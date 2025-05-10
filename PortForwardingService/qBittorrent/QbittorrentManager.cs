#nullable enable

using PortForwardingService.PrivateInternetAccess;
using PortForwardingService.qBittorrent.Data;
using PortForwardingService.qBittorrent.ListeningPortEditors;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unfucked;

namespace PortForwardingService.qBittorrent;

public class QbittorrentManager: IDisposable {

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
                        Console.WriteLine(
                            $"qBittorrent connection state is {transferInfo.connectionStatus} instead of {TransferInfo.ConnectionStatus.CONNECTED}, which means it likely failed to listen on the given IP address and port.");
                        ushort temporaryListeningPort = (ushort) (correctListeningPort < ushort.MaxValue ? correctListeningPort + 1 : correctListeningPort - 1);
                        Console.WriteLine($"Setting qBittorrent listening port to {temporaryListeningPort} and then to {correctListeningPort} in order to try to fix the socket listening error.");

                        await webApiListeningPortEditor.setListeningPort(temporaryListeningPort);
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        await webApiListeningPortEditor.setListeningPort(correctListeningPort);
                    } else {
                        Console.WriteLine($"qBittorrent connection status is {transferInfo?.connectionStatus}, which does not indicate a socket error, ignoring.");
                    }
                } catch (HttpRequestException e) {
                    Console.WriteLine("Failed to check qBittorrent connection state due to error: " + e.MessageChain());
                }
            } else {
                Console.WriteLine("qBittorrent is not running, not checking for socket errors.");
            }
        }, null, SOCKET_ERROR_CHECK_INTERVAL, SOCKET_ERROR_CHECK_INTERVAL);
    }

    public void Dispose() {
        qbittorrentClient.Dispose();
        timer?.Dispose();
    }

}