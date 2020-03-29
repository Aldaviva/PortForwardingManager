#nullable enable

using System;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;

namespace PortForwardingService.ListeningPortEditors {

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
            IniData configContents = readConfigurationFile();
            return Convert.ToUInt16(configContents[SECTION_NAME][LISTENING_PORT_ENTRY_NAME]);
        }

        private IniData readConfigurationFile() {
            return iniFileEditor.ReadFile(CONFIGURATION_FILE_PATH, Encoding.UTF8);
        }

    }

}