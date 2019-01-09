using System;
using System.IO;

namespace PortForwardingManager.μTorrent
{
    public class μTorrentData
    {
        internal virtual string ExecutableFilename => @"uTorrent.exe";
        internal virtual string SettingsFilename => @"settings.dat";
        internal virtual string SettingsKeyListeningPort => @"bind_port";
        public virtual string InstallationDirectory => Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\uTorrent\");
        internal string ExecutableBasename => Path.GetFileNameWithoutExtension(ExecutableFilename);
        internal string AbsoluteExecutablePath => Path.Combine(InstallationDirectory, ExecutableFilename);
    }
}