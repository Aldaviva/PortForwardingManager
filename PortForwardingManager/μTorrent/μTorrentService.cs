using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PortForwardingManager.PIA;

namespace PortForwardingManager.μTorrent {

    public interface μTorrentService {

        bool isμTorrentAlreadyRunning();

        /// <summary>
        /// Modify the <c>settings.dat</c> file of μTorrent to set the listening port to the provided value.
        /// </summary>
        /// <param name="listeningPort"></param>
        /// <exception cref="μTorrentException.NoListeningPort">if the μTorrent settings file doesn't have a listening port set. In this case, open μTorrent, specify a port in Preferences › Connection, and exit, then try running this again.</exception>
        void setμTorrentListeningPort(ushort listeningPort);

        void launchμTorrent(IEnumerable<string> args);

    }

    public class μTorrentServiceImpl: μTorrentService {

        internal LocalProcessService LocalProcessService = new LocalProcessServiceImpl();
        internal μTorrentData μTorrentData = new μTorrentData();

        public bool isμTorrentAlreadyRunning() {
            return LocalProcessService.getProcessesByName(μTorrentData.executableBasename).Length > 0;
        }

        // not using BEncode.NET because μTorrent rejects settings.dat files encoded by that library
        public void setμTorrentListeningPort(ushort listeningPort) {
            string bindPortStartQuery = $":{μTorrentData.settingsKeyListeningPort}i";
            string settingsFile = Path.Combine(μTorrentData.installationDirectory, μTorrentData.settingsFilename);

            byte[] originalBytes = File.ReadAllBytes(settingsFile);
            string expectedSubstring = $"{bindPortStartQuery}{listeningPort}e";

            if (!Encoding.ASCII.GetString(originalBytes).Contains(expectedSubstring)) {
                // remove checksum at beginning of file so μTorrent doesn't reject our changes
                byte[] withFileGuardDeleted = originalBytes.splice(1, 56).ToArray();
                string withFileGuardDeletedAscii = Encoding.ASCII.GetString(withFileGuardDeleted);

                // change the listening port
                int bindPortStart = withFileGuardDeletedAscii.IndexOf(bindPortStartQuery, StringComparison.Ordinal) +
                                    bindPortStartQuery.Length;
                if (bindPortStart == -1) {
                    throw new μTorrentException.NoListeningPort();
                }

                int bindPortEnd = withFileGuardDeletedAscii.IndexOf('e', bindPortStart);
                byte[] portEncoded = Encoding.ASCII.GetBytes(listeningPort.ToString());

                byte[] modifiedBytes = withFileGuardDeleted.splice(bindPortStart, bindPortEnd - bindPortStart, portEncoded)
                    .ToArray();
                File.WriteAllBytes(settingsFile, modifiedBytes);
            }
        }

        public void launchμTorrent(IEnumerable<string> args) {
            string arguments = CommandLine.argvToCommandLine(args);
            LocalProcessService.start(μTorrentData.absoluteExecutablePath, arguments);
        }

    }

    public class μTorrentException: Exception {

        private μTorrentException() { }

        public class NoListeningPort: μTorrentException { }

    }

}