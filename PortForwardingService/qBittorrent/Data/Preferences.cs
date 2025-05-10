#nullable enable

using System.Text.Json.Serialization;

namespace PortForwardingService.qBittorrent.Data;

// Other preferences are excluded
// See full list at https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-4.1)#get-application-preferences
internal class Preferences {

    [JsonPropertyName("listen_port")]
    public ushort listeningPort { get; set; }

}