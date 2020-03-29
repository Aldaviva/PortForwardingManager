#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;
using KoKo.Property;
using Newtonsoft.Json;

namespace PortForwardingService {

    public partial class Service: ServiceBase {

        private readonly StoredProperty<ushort?> piaForwardedPort = new StoredProperty<ushort?>();
        private Process? piaMonitorProcess;
        private volatile bool isShutDown = false;
        private readonly ListeningPortEditor webApiListeningPortEditor = new WebApiListeningPortEditor();
        private readonly ListeningPortEditor configurationFileListeningPortEditor = new ConfigurationFileListeningPortEditor();

        private readonly string qBittorrentExecutablePath =
            Environment.ExpandEnvironmentVariables(@"%programfiles%\qBittorrent\qbittorrent.exe");

        public Service() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            isShutDown = false;

            piaForwardedPort.PropertyChanged += (sender, eventArgs) => {
                Console.WriteLine($"PIA forwarded port changed to {eventArgs.NewValue?.ToString() ?? "null"}");

                ushort? qBittorrentConfiguredListeningPort = getQbittorrentConfigurationListeningPort();
                if (eventArgs.NewValue is ushort forwardedPort && forwardedPort != qBittorrentConfiguredListeningPort) {
                    setQbittorrentListeningPort(forwardedPort);
                }
            };

            Task.Run(listenForPiaPortForwardChanges);
            Console.WriteLine("Listening for forwarded port changes from PIA...");
        }

        private void setQbittorrentListeningPort(ushort listeningPort) {
            ListeningPortEditor listeningPortEditor = isQbittorrentRunning()
                ? webApiListeningPortEditor
                : configurationFileListeningPortEditor;

            listeningPortEditor.setListeningPort(listeningPort);
        }

        private bool isQbittorrentRunning() {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(qBittorrentExecutablePath)).Length > 0;
        }

        private ushort? getQbittorrentConfigurationListeningPort() => configurationFileListeningPortEditor.getListeningPort();

        private void listenForPiaPortForwardChanges() {
            string piaCtlPath = Environment.ExpandEnvironmentVariables(@"%programfiles%\Private Internet Access\piactl.exe");

            var startInfo = new ProcessStartInfo(piaCtlPath) {
                Arguments = "monitor portforward",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            piaMonitorProcess = Process.Start(startInfo);

            if (piaMonitorProcess == null) {
                throw new PrivateInternetAccessException.UnknownForwardedPort();
            }

            piaMonitorProcess.OutputDataReceived += (sender, args) => {
                try {
                    switch (args.Data) {
                        case "Inactive":
                        case "Attempting":
                        case "Unavailable":
                            throw new PrivateInternetAccessException.PortForwardingDisabled();
                        case "Failed":
                            throw new PrivateInternetAccessException.UnknownForwardedPort();
                        default:
                            try {
                                piaForwardedPort.Value = Convert.ToUInt16(args.Data.Trim());
                            } catch (FormatException) {
                                throw new PrivateInternetAccessException.UnknownForwardedPort();
                            } catch (OverflowException) {
                                throw new PrivateInternetAccessException.UnknownForwardedPort();
                            }

                            break;
                    }
                } catch (PrivateInternetAccessException e) {
                    piaForwardedPort.Value = null;
                }
            };

            piaMonitorProcess.BeginOutputReadLine(); //required for OutputDataReceived events to be emitted

            piaMonitorProcess.Exited += (sender, args) => {
                if (!isShutDown) {
                    listenForPiaPortForwardChanges();
                }
            };
        }

        protected override void OnStop() {
            isShutDown = true;
            piaMonitorProcess?.Kill();
            piaMonitorProcess?.Dispose();
        }

        internal void onStart(string[] args) {
            OnStart(args);
        }

        internal void onStop() {
            OnStop();
        }

    }

    internal interface ListeningPortEditor {

        Task setListeningPort(ushort listeningPort);

        ushort? getListeningPort();

    }

    internal class ConfigurationFileListeningPortEditor: ListeningPortEditor {

        private const string SECTION_NAME = "Preferences";
        private const string LISTENING_PORT_ENTRY_NAME = @"Connection\PortRangeMin";

        private static readonly string CONFIGURATION_FILE_PATH =
            Environment.ExpandEnvironmentVariables(@"%appdata%\qBittorrent\qBittorrent.ini");

        private readonly FileIniDataParser iniFileEditor =
            new FileIniDataParser(new IniDataParser(new IniParserConfiguration { AssigmentSpacer = string.Empty }));

        public Task setListeningPort(ushort listeningPort) {
            IniData configContents = readConfigurationFile();

            configContents[SECTION_NAME][LISTENING_PORT_ENTRY_NAME] = Convert.ToString(listeningPort);

            iniFileEditor.WriteFile(CONFIGURATION_FILE_PATH, configContents);
            Console.WriteLine($"Set qBittorrent listening port to {listeningPort} using configuration file.");

            return Task.CompletedTask;
        }

        public ushort? getListeningPort() {
            IniData configurationFile = readConfigurationFile();
            return Convert.ToUInt16(configurationFile[SECTION_NAME][LISTENING_PORT_ENTRY_NAME]);
        }

        private IniData readConfigurationFile() {
            return iniFileEditor.ReadFile(CONFIGURATION_FILE_PATH, Encoding.UTF8);
        }

    }

    internal class WebApiListeningPortEditor: ListeningPortEditor {

        private readonly HttpClient httpClient = new HttpClient();

        public async Task setListeningPort(ushort listeningPort) {
            var requestBodyJsonObject = new {
                listen_port = listeningPort
            };

            var requestBody = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("json", JsonConvert.SerializeObject(requestBodyJsonObject))
            });

            HttpResponseMessage response = await httpClient.PostAsync("http://localhost:8080/api/v2/app/setPreferences", requestBody);

            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Set qBittorrent listening port to {listeningPort} using Web API.");
        }

        public ushort? getListeningPort() {
            throw new NotImplementedException();
        }

    }

}