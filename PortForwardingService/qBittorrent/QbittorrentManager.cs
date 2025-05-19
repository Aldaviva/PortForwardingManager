#nullable enable

using NLog;
using PortForwardingService.PrivateInternetAccess;
using PortForwardingService.qBittorrent.Data;
using PortForwardingService.qBittorrent.ListeningPortEditors;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PortForwardingService.qBittorrent;

public class QbittorrentManager: IDisposable {

    private static readonly Logger   LOGGER                      = LogManager.GetLogger(typeof(QbittorrentManager).FullName);
    private static readonly TimeSpan SOCKET_ERROR_CHECK_INTERVAL = TimeSpan.FromMinutes(3);

    private readonly string                  qBittorrentExecutablePath            = Environment.ExpandEnvironmentVariables(@"%programfiles%\qBittorrent\qbittorrent.exe");
    private readonly QbittorrentClient       qbittorrentClient                    = new();
    private readonly ListeningPortEditor     configurationFileListeningPortEditor = new ConfigurationFileListeningPortEditor();
    private readonly ListeningPortEditor     webApiListeningPortEditor;
    private readonly PiaForwardedPortMonitor piaForwardedPortMonitor;

    private Timer? timer;

    public QbittorrentManager(PiaForwardedPortMonitor piaForwardedPortMonitor) {
        this.piaForwardedPortMonitor = piaForwardedPortMonitor;
        webApiListeningPortEditor    = new WebApiListeningPortEditor(qbittorrentClient);
    }

    public Task<ushort?> getQbittorrentConfigurationListeningPort() => configurationFileListeningPortEditor.getListeningPort();

    public async Task setQbittorrentListeningPort(ushort listeningPort) {
        ListeningPortEditor listeningPortEditor = isQbittorrentRunning()
            ? webApiListeningPortEditor
            : configurationFileListeningPortEditor;

        await listeningPortEditor.setListeningPort(listeningPort);
    }

    private bool isQbittorrentRunning() {
        Process[] qBittorrentProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(qBittorrentExecutablePath));
        foreach (Process process in qBittorrentProcesses) {
            process.Dispose();
        }

        return qBittorrentProcesses.Length > 0;
    }

    public void listenForSocketErrors() {
        timer = new Timer(async _ => {
            if (isQbittorrentRunning()) {
                try {
                    TransferInfo? transferInfo = await qbittorrentClient.send<TransferInfo>(HttpMethod.Get, "transfer/info");

                    if (transferInfo?.connectionStatus is not (TransferInfo.ConnectionStatus.CONNECTED or null) && piaForwardedPortMonitor.forwardedPort.Value is { } correctListeningPort) {
                        LOGGER.Info("qBittorrent connection state is {actual} instead of {expected}, which means it likely failed to listen on the given IP address and port.",
                            transferInfo.connectionStatus, TransferInfo.ConnectionStatus.CONNECTED);
                        ushort temporaryListeningPort = (ushort) (correctListeningPort < ushort.MaxValue ? correctListeningPort + 1 : correctListeningPort - 1);
                        LOGGER.Info("Setting qBittorrent listening port to {temp} and then to {real} in order to try to fix the socket listening error.", temporaryListeningPort, correctListeningPort);

                        await webApiListeningPortEditor.setListeningPort(temporaryListeningPort);
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        await webApiListeningPortEditor.setListeningPort(correctListeningPort);
                    } else {
                        LOGGER.Debug("qBittorrent connection status is {status}, which does not indicate a socket error, ignoring.", transferInfo?.connectionStatus);
                    }
                } catch (HttpRequestException e) {
                    LOGGER.Warn(e, "Failed to check qBittorrent connection state due to error.");
                }
            } else {
                LOGGER.Debug("qBittorrent is not running, not checking for socket errors.");
            }
        }, null, SOCKET_ERROR_CHECK_INTERVAL, SOCKET_ERROR_CHECK_INTERVAL);
    }

    public void Dispose() {
        timer?.Dispose();
        qbittorrentClient.Dispose();
    }

}