using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using PortForwardingManager.PIA;
using PortForwardingManager.μTorrent;

namespace PortForwardingManager {

    public class PortForwardingManager {

        internal PrivateInternetAccessService PrivateInternetAccessService = new ControlForkingPrivateInternetAccessService();
        internal μTorrentService μTorrentService = new μTorrentServiceImpl();
        internal ErrorReportingService ErrorReportingService = new ErrorReportingServiceImpl();

        public static void Main(string[] args) {
            Application.EnableVisualStyles();
            new PortForwardingManager().updateSettingsAndLaunch(args);
        }

        internal void updateSettingsAndLaunch(IEnumerable<string> args) {
            if (!μTorrentService.isμTorrentAlreadyRunning()) {
                try {
                    ushort listeningPort = PrivateInternetAccessService.getPrivateInternetAccessForwardedPort();
                    μTorrentService.setμTorrentListeningPort(listeningPort);
                } catch (PrivateInternetAccessException.UnknownForwardedPort) {
                    ErrorReportingService.reportError("Error reading PIA forwarded port",
                        "Could not read forwarded port from Private Internet Access control program.\r\n\r\n" +
                        "Make sure Private Internet Access v1.6 or later is installed.", MessageBoxIcon.Warning);
                } catch (PrivateInternetAccessException.NoDaemonLogFile) {
                    ErrorReportingService.reportError("PIA debug logging is disabled",
                        "Failed to read log file.\r\n\r\nMake sure Debug Logging is enabled on the Help page of PIA's Settings.",
                        MessageBoxIcon.Warning);
                } catch (PrivateInternetAccessException.PortForwardingDisabled) {
                    ErrorReportingService.reportError("PIA port forwarding is disabled",
                        "Port forwarding is disabled in Private Internet Access.\r\n\r\n" +
                        "Make sure PIA has \"Request port forwarding\" enabled on the Network page of PIA's settings, " +
                        "and connect to one of the servers that supports port forwarding, like Toronto or Vancouver.",
                        MessageBoxIcon.Warning);
                }
            }

            try {
                μTorrentService.launchμTorrent(args);
            } catch (FileNotFoundException e) {
                ErrorReportingService.reportError("Error starting μTorrent",
                    $"μTorrent could not be started because the file {e.FileName} was not found.\r\n\r\n" +
                    "Make sure μTorrent is installed in that directory.", MessageBoxIcon.Error);
            }
        }

    }

}