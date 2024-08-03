#nullable enable

using PortForwardingService.qBittorrent.Data;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PortForwardingService.qBittorrent.ListeningPortEditors;

internal class WebApiListeningPortEditor(QbittorrentClient client): ListeningPortEditor {

    public async Task setListeningPort(ushort listeningPort) {
        await client.send(HttpMethod.Post, "app/setPreferences", new Preferences { listeningPort = listeningPort });

        Console.WriteLine($"Set qBittorrent listening port to {listeningPort} using Web API.");
    }

    public async Task<ushort?> getListeningPort() {
        Preferences? preferences = await client.send<Preferences>(HttpMethod.Get, "app/preferences");
        return preferences?.listeningPort;
    }

}