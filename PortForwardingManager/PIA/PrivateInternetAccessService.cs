using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
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

        /// <inheritdoc />
        /// <remarks>This isn't tested and error cases aren't handled cleanly because it's a temporary fix until the rewritten
        /// PIA client is released (which is in beta as of 2019-01-08).</remarks>
        public ushort GetPrivateInternetAccessForwardedPort()
        {
            string logFileName = Path.Combine(PrivateInternetAccessData.LogDirectory, "pia_manager.log");
            string logFileContents;
            try
            {
                using (FileStream fileStream = File.Open(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream))
                {
                    logFileContents = reader.ReadToEnd();
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Failed to read log file");
                throw;
            }

            Match match = Regex.Match(logFileContents, PrivateInternetAccessData.LOG_PATTERN, RegexOptions.RightToLeft);
            return ushort.Parse(match.Groups[1].Value);
        }

        /// <summary>
        /// Private Internet Access v82 for Windows removed the tooltip "for consistency across platforms"
        /// </summary>
        /// <remarks>https://www.privateinternetaccess.com/pages/downloads#v82</remarks>
        [Obsolete("Use GetPrivateInternetAccessForwardedPort() instead")]
        public ushort GetPrivateInternetAccessForwardedPortWithTooltip()
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