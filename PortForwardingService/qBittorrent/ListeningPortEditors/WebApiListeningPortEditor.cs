#nullable enable

using NLog;
using qBittorrent.Client;
using qBittorrent.Client.Data;

namespace PortForwardingService.qBittorrent.ListeningPortEditors;

internal sealed class WebApiListeningPortEditor(qBittorrentClient client): ListeningPortEditor {

    private static readonly Logger LOGGER = LogManager.GetLogger(typeof(WebApiListeningPortEditor).FullName!);

    public async Task setListeningPort(ushort listeningPort) {
        await client.setPreferences(new Preferences { listeningPort = listeningPort });
        LOGGER.Info("Set qBittorrent listening port to {listeningPort} using Web API.", listeningPort);
    }

    public async Task<ushort?> getListeningPort() => (await client.getPreferences()).listeningPort;

}