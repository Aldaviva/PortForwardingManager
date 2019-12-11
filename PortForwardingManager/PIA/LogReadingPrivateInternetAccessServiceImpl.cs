using System.IO;
using System.Text.RegularExpressions;

namespace PortForwardingManager.PIA {

    /// <summary>
    /// Read the PIA forwarded port by scraping the debug log files "data\daemon.log" and "data\daemon.log.old". This works on all
    /// versions of PIA, including versions less than v1.6, but there are disadvantages as well.
    ///
    /// It requires the user to manually enable Debug Logging in the PIA settings and leave it on forever, and if PIA has a high enough
    /// connection uptime for the port forwarding enablement log message to be rotated out of the log files, then this class won't be
    /// able to read it. The log files are also not a stable, programmatic interface, and therefore the format of the messages may
    /// change, requiring alteration of the pattern used to match the port number. This happened when PIA updated from v82 to v1.0.
    /// </summary>
    public class LogReadingPrivateInternetAccessServiceImpl: PrivateInternetAccessService {

        public ushort getPrivateInternetAccessForwardedPort() {
            string[] logFilenames = {
                Path.Combine(PrivateInternetAccessData.dataDirectory, "daemon.log"), // most recent, rolls over faster, smaller (~300 KB)
                Path.Combine(PrivateInternetAccessData.dataDirectory, "daemon.log.old") // less recent, larger (~4 MB)
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

                Match match = PrivateInternetAccessData.LOG_PATTERN.Match(logFileContents);
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

}