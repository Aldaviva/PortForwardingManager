#nullable enable
using Newtonsoft.Json;

namespace PortForwardingService.qBittorrent.Data;

internal class TransferInfo {

    [JsonProperty("connection_status")]
    public ConnectionStatus connectionStatus { get; set; }

    [JsonProperty("dht_nodes")]
    public uint dhtNodes { get; set; }

    [JsonProperty("dl_info_data")]
    public ulong dlInfoData { get; set; }

    [JsonProperty("dl_info_speed")]
    public uint dlInfoSpeed { get; set; }

    [JsonProperty("dl_rate_limit")]
    public uint dlRateLimit { get; set; }

    [JsonProperty("up_info_data")]
    public ulong upInfoData { get; set; }

    [JsonProperty("up_info_speed")]
    public uint upInfoSpeed { get; set; }

    [JsonProperty("up_rate_limit")]
    public uint upRateLimit { get; set; }

    internal enum ConnectionStatus {

        CONNECTED,
        FIREWALLED,
        DISCONNECTED

    }

}