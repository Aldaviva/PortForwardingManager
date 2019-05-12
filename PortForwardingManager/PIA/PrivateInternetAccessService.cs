using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PortForwardingManager.PIA
{
    public interface PrivateInternetAccessService
    {
        /// <summary>
        /// Figure out which port, if any, is being port-forwarded by Private Internet Access.
        /// </summary>
        /// <returns>The public WAN listening port number that is being forwarded to your VPN client</returns>
        /// <exception cref="PrivateInternetAccessException.NoForwardedPort">If PIA is running but not forwarding any
        /// ports (possibly because it's still starting up, it's connected to a VPN server that does not allow port
        /// forwarding, or the local PIA settings have port forwarding disabled).</exception>
        /// <exception cref="PrivateInternetAccessException.NoDaemonLogFile">If the daemon debug log file does not
        /// exist because PIA debug logging is disabled.</exception>
        ushort GetPrivateInternetAccessForwardedPort();
    }

    public class PrivateInternetAccessServiceImpl : PrivateInternetAccessService
    {
        public ushort GetPrivateInternetAccessForwardedPort()
        {
            string logFileName = Path.Combine(PrivateInternetAccessData.DataDirectory, "daemon.log");
            string logFileContents;
            try
            {
                // Using the simpler File.ReadAllText() throws an IOException because the log file is locked for writing by PIA.
                using (FileStream fileStream = File.Open(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream))
                {
                    logFileContents = reader.ReadToEnd();
                }
            }
            catch (IOException)
            {
                throw new PrivateInternetAccessException.NoDaemonLogFile();
            }

            Match match = Regex.Match(logFileContents, PrivateInternetAccessData.LOG_PATTERN, RegexOptions.RightToLeft);
            if (match.Success)
            {
                int forwardedPortNumber = int.Parse(match.Groups[1].Value);
                if (forwardedPortNumber > 0)
                {
                    return (ushort) forwardedPortNumber;
                }
                else
                {
                    // forwarded port is -1 or 0, which means port forwarding is not enabled
                    throw new PrivateInternetAccessException.NoForwardedPort();
                }
            }
            else
            {
                // no port forwarding log statements found
                throw new PrivateInternetAccessException.NoForwardedPort();
            }
        }
    }

    public class PrivateInternetAccessException : Exception
    {
        private PrivateInternetAccessException()
        {
        }

        public class NoForwardedPort : PrivateInternetAccessException
        {
        }

        public class NoDaemonLogFile : PrivateInternetAccessException
        {
        }
    }
}