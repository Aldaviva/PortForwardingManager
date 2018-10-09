using System;
using System.Linq;
using System.Text.RegularExpressions;
using NotificationArea;

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
        /// <exception cref="PrivateInternetAccessException.NoNotificationIcon">If PIA is not running or its tray
        /// icon is hidden.</exception>
        ushort GetPrivateInternetAccessForwardedPort();
    }

    public class PrivateInternetAccessServiceImpl : PrivateInternetAccessService
    {
        internal NotificationArea.NotificationArea NotificationArea = new NotificationAreaImpl();

        public ushort GetPrivateInternetAccessForwardedPort()
        {
            try
            {
                NotificationIcon μTorrentNotificationIcon = NotificationArea.NotificationIcons.First(
                    notificationIcon => notificationIcon.ProcessName == PrivateInternetAccessData.ExecutableBasename);

                Match match = Regex.Match(μTorrentNotificationIcon.ToolTip, PrivateInternetAccessData.TOOLTIP_PATTERN);

                if (!match.Success)
                {
                    throw new PrivateInternetAccessException.NoForwardedPort();
                }

                return ushort.Parse(match.Groups[1].Value);
            }
            catch (InvalidOperationException)
            {
                throw new PrivateInternetAccessException.NoNotificationIcon();
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

        public class NoNotificationIcon : PrivateInternetAccessException
        {
        }
    }
}