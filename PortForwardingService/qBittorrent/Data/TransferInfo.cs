#nullable enable

using System.Text.Json.Serialization;

namespace PortForwardingService.qBittorrent.Data;

internal class TransferInfo {

    [JsonPropertyName("connection_status")]
    public ConnectionStatus connectionStatus { get; set; }

    [JsonPropertyName("dht_nodes")]
    public uint dhtNodes { get; set; }

    [JsonPropertyName("dl_info_data")]
    public ulong dlInfoData { get; set; }

    [JsonPropertyName("dl_info_speed")]
    public uint dlInfoSpeed { get; set; }

    [JsonPropertyName("dl_rate_limit")]
    public uint dlRateLimit { get; set; }

    [JsonPropertyName("up_info_data")]
    public ulong upInfoData { get; set; }

    [JsonPropertyName("up_info_speed")]
    public uint upInfoSpeed { get; set; }

    [JsonPropertyName("up_rate_limit")]
    public uint upRateLimit { get; set; }

    internal enum ConnectionStatus {

        CONNECTED,
        FIREWALLED,
        DISCONNECTED

    }

}