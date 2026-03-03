#nullable enable

namespace PortForwardingService.Plugins.SamplePlugin;

public class SamplePlugin: IPortForwardingServicePlugin, IDisposable {

    public SamplePlugin() {
        // TODO: initialization logic goes here
    }

    public void OnForwardedPortChanged(ushort? newForwardedPort, ushort? oldForwardedPort) {
        // TODO: forwarded port has changed
    }

    public void Dispose() {
        // TODO: release managed resources here
        GC.SuppressFinalize(this);
    }

}