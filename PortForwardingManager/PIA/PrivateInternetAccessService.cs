using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PortForwardingManager.PIA {

    public interface PrivateInternetAccessService {

        /// <summary>
        /// Figure out which port, if any, is being port-forwarded by Private Internet Access.
        /// </summary>
        /// <returns>The public WAN listening port number that is being forwarded to your VPN client</returns>
        /// <exception cref="PrivateInternetAccessException.UnknownForwardedPort">If PIA is running but not forwarding any
        /// ports (possibly because it's still starting up, disconnected, or the logs have already rotated since connecting).</exception>
        /// <exception cref="PrivateInternetAccessException.NoDaemonLogFile">If the daemon debug log file does not
        /// exist because PIA debug logging is disabled or the installation directory is incorrect.</exception>
        /// <exception cref="PrivateInternetAccessException.PortForwardingDisabled">If PIA has port forwarding disabled, or is
        /// connected to a server that does not allow port forwarding.</exception>
        ushort GetPrivateInternetAccessForwardedPort();

    }

    public class PrivateInternetAccessServiceImpl: PrivateInternetAccessService {

        public ushort GetPrivateInternetAccessForwardedPort() {
            string[] logFilenames = {
                Path.Combine(PrivateInternetAccessData.DataDirectory, "daemon.log"), // most recent, rolls over faster, smaller (~300 KB)
                Path.Combine(PrivateInternetAccessData.DataDirectory, "daemon.log.old") // less recent, larger (~4 MB)
            };

            foreach (string logFileName in logFilenames) {
                string logFileContents;
                try {
                    // Using the simpler File.ReadAllText() throws an IOException because the log file is locked for writing by PIA.
                    using (FileStream fileStream = File.Open(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fileStream)) {
                        logFileContents = reader.ReadToEnd();
                    }
                } catch (IOException) {
                    throw new PrivateInternetAccessException.NoDaemonLogFile();
                }

                Match match = PrivateInternetAccessData.LogPattern.Match(logFileContents);
                if (match.Success) {
                    int forwardedPortNumber = int.Parse(match.Groups[1].Value);
                    if (forwardedPortNumber > 0) {
                        return (ushort) forwardedPortNumber;
                    } else {
                        // forwarded port is -1 or 0, which means port forwarding is not enabled
                        throw new PrivateInternetAccessException.PortForwardingDisabled();
                    }

                    // otherwise, continue to next log file
                }
            }

            // no port forwarding log statements found,
            // possibly because the connection was started a very long time ago and the logs rolled over already
            throw new PrivateInternetAccessException.UnknownForwardedPort();
        }

    }

    public class PrivateInternetAccessException: Exception {

        private PrivateInternetAccessException() { }

        public class UnknownForwardedPort: PrivateInternetAccessException { }

        public class NoDaemonLogFile: PrivateInternetAccessException { }

        public class PortForwardingDisabled: PrivateInternetAccessException { }

    }

}