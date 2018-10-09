using System.Collections.Generic;
using PortForwardingManager.PIA;
using PortForwardingManager.μTorrent;

namespace PortForwardingManager
{
    public class PortForwardingManager
    {
        internal PrivateInternetAccessService PrivateInternetAccessService = new PrivateInternetAccessServiceImpl();
        internal μTorrentService μTorrentService = new μTorrentServiceImpl();
        internal ErrorReportingService ErrorReportingService = new ErrorReportingServiceImpl();

        public static void Main(string[] args)
        {
            new PortForwardingManager().UpdateSettingsAndLaunch(args);
        }

        internal void UpdateSettingsAndLaunch(IEnumerable<string> args)
        {
            if (!μTorrentService.IsμTorrentAlreadyRunning())
            {
                try
                {
                    ushort listeningPort = PrivateInternetAccessService.GetPrivateInternetAccessForwardedPort();
                    μTorrentService.SetμTorrentListeningPort(listeningPort);
                }
                catch (PrivateInternetAccessException.NoForwardedPort)
                {
                    ErrorReportingService.ReportError("Error setting μTorrent listening port",
                        "Could not read forwarded port from Private Internet Access notification icon tooltip.\r\n\r\n" +
                        "Make sure PIA has \"Request port forwarding\" enabled and is connected to one of the regions that supports port forwarding, like CA Toronto.");
                }
                catch (PrivateInternetAccessException.NoNotificationIcon)
                {
                    ErrorReportingService.ReportError("Error setting μTorrent listening port",
                        "Could not find Private Internet Access icon in the notification area.\r\n\r\nMake sure PIA is running.");
                }
            }

            μTorrentService.LaunchμTorrent(args);
        }
    }
}