using System;
using System.IO;

namespace PortForwardingManager.μTorrent
{
    public class μTorrentData
    {
        internal virtual string executableFilename => @"uTorrent.exe";
        internal virtual string settingsFilename => @"settings.dat";
        internal virtual string settingsKeyListeningPort => @"bind_port";
        public virtual string installationDirectory => Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\uTorrent\");
        internal string executableBasename => Path.GetFileNameWithoutExtension(executableFilename);
        internal string absoluteExecutablePath => Path.Combine(installationDirectory, executableFilename);
    }
}