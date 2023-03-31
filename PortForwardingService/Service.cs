#nullable enable

using System;
using System.ServiceProcess;

namespace PortForwardingService;

public partial class Service: ServiceBase {

    private readonly PiaForwardedPortMonitor piaForwardedPortMonitor = new();
    private readonly QbittorrentManager      qBittorrentManager      = new();

    public Service() {
        InitializeComponent();
    }

    protected override void OnStart(string[] args) {
        piaForwardedPortMonitor.forwardedPort.PropertyChanged += (_, eventArgs) => {
            Console.WriteLine($"PIA forwarded port changed to {eventArgs.NewValue?.ToString() ?? "null"}");

            ushort? qBittorrentListeningPort = qBittorrentManager.getQbittorrentConfigurationListeningPort();

            if (eventArgs.NewValue is { } piaForwardedPort && piaForwardedPort != qBittorrentListeningPort) {
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