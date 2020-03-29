#nullable enable

using System;
using System.ServiceProcess;

namespace PortForwardingService {

    public partial class Service: ServiceBase {

        private readonly PiaForwardedPortMonitor piaForwardedPortMonitor = new PiaForwardedPortMonitor();
        private readonly QbittorrentManager qBittorrentManager = new QbittorrentManager();

        public Service() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            piaForwardedPortMonitor.forwardedPort.PropertyChanged += (sender, eventArgs) => {
                Console.WriteLine($"PIA forwarded port changed to {eventArgs.NewValue?.ToString() ?? "null"}");

                ushort? qBittorrentListeningPort = qBittorrentManager.getQbittorrentConfigurationListeningPort();

                if (eventArgs.NewValue is ushort piaForwardedPort && piaForwardedPort != qBittorrentListeningPort) {
                    qBittorrentManager.setQbittorrentListeningPort(piaForwardedPort);
                }
            };

            piaForwardedPortMonitor.listenForPiaPortForwardChanges();
            Console.WriteLine("Listening for forwarded port changes from PIA...");
        }

        protected override void OnStop() {
            piaForwardedPortMonitor.Dispose();
        }

        internal void onStart(string[] args) {
            OnStart(args);
        }

        internal void onStop() {
            OnStop();
        }

    }

}