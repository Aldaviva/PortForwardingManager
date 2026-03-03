#nullable enable

using NLog;
using Plugins;
using PortForwardingService.Plugins;
using PortForwardingService.PrivateInternetAccess;
using PortForwardingService.qBittorrent;
using System.ServiceProcess;

namespace PortForwardingService;

public partial class Service: ServiceBase {

    private static readonly Logger LOGGER = LogManager.GetLogger(typeof(Service).FullName!);

    private readonly PiaForwardedPortMonitor                      piaForwardedPortMonitor = new();
    private readonly QbittorrentManager                           qBittorrentManager;
    private readonly IPluginManager<IPortForwardingServicePlugin> pluginManager = new PluginManager<IPortForwardingServicePlugin>("plugins");

    public Service() {
        qBittorrentManager = new QbittorrentManager(piaForwardedPortMonitor);
        InitializeComponent();
    }

    protected override void OnStart(string[] args) {
        pluginManager.LoadAll();

        piaForwardedPortMonitor.forwardedPort.PropertyChanged += async (_, eventArgs) => {
            LOGGER.Info("PIA forwarded port changed to {newPort}", eventArgs.NewValue?.ToString() ?? "null");

            ushort? qBittorrentListeningPort = await qBittorrentManager.getQbittorrentConfigurationListeningPort();

            if (eventArgs.NewValue is {} piaForwardedPort && piaForwardedPort != qBittorrentListeningPort) {
                await qBittorrentManager.setQbittorrentListeningPort(piaForwardedPort);
            }

            foreach (IPortForwardingServicePlugin plugin in pluginManager.Plugins) {
                plugin.OnForwardedPortChanged(eventArgs.NewValue, eventArgs.OldValue);
            }
        };

        piaForwardedPortMonitor.listenForPiaPortForwardChanges();
        LOGGER.Info("Listening for forwarded port changes from PIA...");

        qBittorrentManager.listenForSocketErrors();
        LOGGER.Info("Listening for socket errors from qBittorrent...");
    }

    protected override void OnStop() {
        pluginManager.Dispose();
        piaForwardedPortMonitor.Dispose();
        qBittorrentManager.Dispose();
        LogManager.Shutdown();
    }

    internal void onStart(string[] args) {
        OnStart(args);
    }

    internal void onStop() {
        OnStop();
    }

}