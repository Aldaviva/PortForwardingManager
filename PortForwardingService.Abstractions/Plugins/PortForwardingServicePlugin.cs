#nullable enable

namespace PortForwardingService.Plugins;

/// <summary>
/// Plugin for PortForwardingService, allowing users to extend the behavior of the program without having to maintain a fork.
/// </summary>
public interface IPortForwardingServicePlugin {

    /// <summary>
    /// Fired when the Private Internet Access forwarded port changes.
    /// </summary>
    /// <param name="newForwardedPort">The current forwarded port number, or <c>null</c> if port forwarding is now disabled.</param>
    /// <param name="oldForwardedPort">The previous forwarded port number, or <c>null</c> if port forwarding was disabled.</param>
    void OnForwardedPortChanged(ushort? newForwardedPort, ushort? oldForwardedPort);

}