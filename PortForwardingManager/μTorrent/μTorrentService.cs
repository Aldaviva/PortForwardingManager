using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PortForwardingManager.μTorrent
{
    public interface μTorrentService
    {
        bool IsμTorrentAlreadyRunning();
        void SetμTorrentListeningPort(ushort listeningPort);
        void LaunchμTorrent(IEnumerable<string> args);
    }

    public class μTorrentServiceImpl : μTorrentService
    {
        internal LocalProcessService LocalProcessService = new LocalProcessServiceImpl();
        internal μTorrentData μTorrentData = new μTorrentData();

        public bool IsμTorrentAlreadyRunning()
        {
            return LocalProcessService.GetProcessesByName(μTorrentData.ExecutableBasename).Length > 0;
        }

        // not using BEncode.NET because μTorrent rejects settings.dat files encoded by that library
        public void SetμTorrentListeningPort(ushort listeningPort)
        {
            string bindPortStartQuery = $":{μTorrentData.SettingsKeyListeningPort}i";
            string settingsFile = Path.Combine(μTorrentData.InstallationDirectory, μTorrentData.SettingsFilename);

            byte[] originalBytes = File.ReadAllBytes(settingsFile);
            string expectedSubstring = $"{bindPortStartQuery}{listeningPort}e";

            if (!Encoding.ASCII.GetString(originalBytes).Contains(expectedSubstring))
            {
                // remove checksum at beginning of file so μTorrent doesn't reject our changes
                byte[] withFileGuardDeleted = originalBytes.Splice(1, 56).ToArray();
                string withFileGuardDeletedAscii = Encoding.ASCII.GetString(withFileGuardDeleted);

                // change the listening port
                int bindPortStart = withFileGuardDeletedAscii.IndexOf(bindPortStartQuery, StringComparison.Ordinal) +
                                    bindPortStartQuery.Length;
                int bindPortEnd = withFileGuardDeletedAscii.IndexOf('e', bindPortStart);
                byte[] portEncoded = Encoding.ASCII.GetBytes(listeningPort.ToString());

                byte[] modifiedBytes = withFileGuardDeleted.Splice(bindPortStart, bindPortEnd - bindPortStart, portEncoded)
                    .ToArray();
                File.WriteAllBytes(settingsFile, modifiedBytes);
            }
        }

        public void LaunchμTorrent(IEnumerable<string> args)
        {
            string arguments = CommandLine.ArgvToCommandLine(args);
            LocalProcessService.Start(Path.Combine(μTorrentData.InstallationDirectory, μTorrentData.ExecutableFilename), arguments);
        }
    }
}