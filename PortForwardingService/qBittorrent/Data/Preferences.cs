#nullable enable

using Newtonsoft.Json;

namespace PortForwardingService.qBittorrent.Data;

// Other preferences are excluded
// See full list at https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-4.1)#get-application-preferences
internal class Preferences {

    [JsonProperty("listen_port")]
    public ushort listeningPort { get; set; }

}