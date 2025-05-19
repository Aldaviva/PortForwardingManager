#nullable enable

using NLog;
using PortForwardingService.qBittorrent.Data;
using System.Net.Http;
using System.Threading.Tasks;

namespace PortForwardingService.qBittorrent.ListeningPortEditors;

internal class WebApiListeningPortEditor(QbittorrentClient client): ListeningPortEditor {

    private static readonly Logger LOGGER = LogManager.GetLogger(typeof(WebApiListeningPortEditor).FullName);

    public async Task setListeningPort(ushort listeningPort) {
        (await client.send(HttpMethod.Post, "app/setPreferences", new Preferences { listeningPort = listeningPort })).Dispose();

        LOGGER.Info("Set qBittorrent listening port to {listeningPort} using Web API.", listeningPort);
    }

    public async Task<ushort?> getListeningPort() {
        Preferences? preferences = await client.send<Preferences>(HttpMethod.Get, "app/preferences");
        return preferences?.listeningPort;
    }

}