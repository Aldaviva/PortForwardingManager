using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PortForwardingManager.PIA
{
    internal struct PrivateInternetAccessData
    {
        internal static readonly Regex LogPattern = new Regex(@"Forwarded port updated to (-?\d{1,5})\b", RegexOptions.RightToLeft);

        internal static string InstallationDirectory = Environment.ExpandEnvironmentVariables(@"%PROGRAMFILES%\Private Internet Access\");
        internal static string DataDirectory => Path.Combine(InstallationDirectory, "data");
    }
}