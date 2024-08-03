#nullable enable

using PortForwardingService.PrivateInternetAccess;
using PortForwardingService.qBittorrent;
using System;
using System.ServiceProcess;

namespace PortForwardingService;

public partial class Service: ServiceBase {

    private readonly PiaForwardedPortMonitor piaForwardedPortMonitor = new();
    private readonly QbittorrentManager      qBittorrentManager;

    public Service() {
        qBittorrentManager = new QbittorrentManager(piaForwardedPortMonitor);
        InitializeComponent();
    }

    protected override void OnStart(string[] args) {
        piaForwardedPortMonitor.forwardedPort.PropertyChanged += async (_, eventArgs) => {
            Console.WriteLine($"PIA forwarded port changed to {eventArgs.NewValue?.ToString() ?? "null"}");

            ushort? qBittorrentListeningPort = await qBittorrentManager.getQbittorrentConfigurationListeningPort();

            if (eventArgs.NewValue is { } piaForwardedPort && piaForwardedPort != qBittorrentListeningPort) {
                await qBittorrentManager.setQbittorrentListeningPort(piaForwardedPort);
            }
        };

        piaForwardedPortMonitor.listenForPiaPortForwardChanges();
        Console.WriteLine("Listening for forwarded port changes from PIA...");

        qBittorrentManager.listenForSocketErrors();
        Console.WriteLine("Listening for socket errors from qBittorrent...");
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