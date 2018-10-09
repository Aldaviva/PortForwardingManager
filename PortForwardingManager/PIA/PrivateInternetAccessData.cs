using System.IO;

namespace PortForwardingManager.PIA
{
    internal struct PrivateInternetAccessData
    {
        private const string EXECUTABLE_FILENAME = @"pia_nw.exe";
        internal const string TOOLTIP_PATTERN =
            @"^Private Internet Access - You are connected \(.+?\) - \((?:\d{1,3}\.){3}\d{1,3}\) \[ Port: (\d{1,5}) \]$";

        internal static string ExecutableBasename => Path.GetFileNameWithoutExtension(EXECUTABLE_FILENAME);
    }
}