using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using PortForwardingManager.PIA;
using PortForwardingManager.μTorrent;

namespace PortForwardingManager {

    public class PortForwardingManager {

        internal PrivateInternetAccessService PrivateInternetAccessService = new PrivateInternetAccessServiceImpl();
        internal μTorrentService μTorrentService = new μTorrentServiceImpl();
        internal ErrorReportingService ErrorReportingService = new ErrorReportingServiceImpl();

        public static void Main(string[] args) {
            Application.EnableVisualStyles();
            new PortForwardingManager().UpdateSettingsAndLaunch(args);
        }

        internal void UpdateSettingsAndLaunch(IEnumerable<string> args) {
            if (!μTorrentService.IsμTorrentAlreadyRunning()) {
                try {
                    ushort listeningPort = PrivateInternetAccessService.GetPrivateInternetAccessForwardedPort();
                    μTorrentService.SetμTorrentListeningPort(listeningPort);
                } catch (PrivateInternetAccessException.UnknownForwardedPort) {
                    ErrorReportingService.ReportError("Error reading PIA forwarded port",
                        "Could not read forwarded port from Private Internet Access log file.\r\n\r\n" +
                        "Make sure PIA has \"Request port forwarding\" enabled and is connected to one of the regions that supports " +
                        "port forwarding, like Toronto or Vancouver.",
                        MessageBoxIcon.Warning);
                } catch (PrivateInternetAccessException.NoDaemonLogFile) {
                    ErrorReportingService.ReportError("PIA debug logging is disabled",
                        "Failed to read log file.\r\n\r\nMake sure Debug Logging is enabled on the Help page of PIA's Settings.",
                        MessageBoxIcon.Warning);
                } catch (PrivateInternetAccessException.PortForwardingDisabled) {
                    ErrorReportingService.ReportError("PIA port forwarding is disabled",
                        "Port forwarding is disabled in Private Internet Access.\r\n\r\n" +
                        "Make sure PIA has \"Request port forwarding\" enabled on the Network page of PIA's settings, " +
                        "and connect to one of the servers that supports port forwarding, like Toronto or Vancouver.",
                        MessageBoxIcon.Warning);
                }
            }

            try {
                μTorrentService.LaunchμTorrent(args);
            } catch (FileNotFoundException e) {
                ErrorReportingService.ReportError("Error starting μTorrent",
                    $"μTorrent could not be started because the file {e.FileName} was not found.\r\n\r\n" +
                    "Make sure μTorrent is installed in that directory.", MessageBoxIcon.Error);
            }
        }

    }

}