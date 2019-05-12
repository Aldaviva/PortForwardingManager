using System;
using System.IO;

namespace PortForwardingManager.PIA
{
    internal struct PrivateInternetAccessData
    {
        internal const string LOG_PATTERN = @"Forwarded port updated to (-?\d{1,5})\b";

        internal static string InstallationDirectory = Environment.ExpandEnvironmentVariables(@"%PROGRAMFILES%\Private Internet Access\");
        internal static string DataDirectory => Path.Combine(InstallationDirectory, "data");
    }
}