using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NotificationArea;

// ReSharper disable InconsistentNaming

namespace PortForwardingManager
{
#pragma warning disable IDE1006 // Naming Styles
    internal struct μTorrent
#pragma warning restore IDE1006 // Naming Styles
    {
        private const string INSTALLATION_DIRECTORY = @"%APPDATA%\uTorrent\";
        internal const string EXECUTABLE_FILENAME = @"uTorrent.exe";
        internal const string SETTINGS_FILENAME = @"settings.dat";
        internal const string SETTINGS_KEY_LISTENING_PORT = @"bind_port";

        internal const string TOOLTIP_PATTERN =
            @"^Private Internet Access - You are connected \(.+?\) - \((?:\d{1,3}\.){3}\d{1,3}\) \[ Port: (\d{1,5}) \]$";

        internal static string InstallationDirectory => Environment.ExpandEnvironmentVariables(INSTALLATION_DIRECTORY);
        internal static string ExecutableBasename => Path.GetFileNameWithoutExtension(EXECUTABLE_FILENAME);
    }

    internal struct PrivateInternetAccess
    {
        private const string EXECUTABLE_FILENAME = @"pia_nw.exe";
        internal static string ExecutableBasename => Path.GetFileNameWithoutExtension(EXECUTABLE_FILENAME);
    }

#pragma warning disable IDE1006 // Naming Styles
    internal static class μTorrentLauncher
#pragma warning restore IDE1006 // Naming Styles
    {
        public static void Main(string[] args)
        {
            UpdateSettingsAndLaunch(args);
        }

        private static void UpdateSettingsAndLaunch(IEnumerable<string> args)
        {
            if (!IsμTorrentAlreadyRunning() && GetPrivateInternetAccessForwardedPort() is int listeningPort)
            {
                SetμTorrentListeningPort(listeningPort);
            }

            LaunchμTorrent(args);
        }

        private static bool IsμTorrentAlreadyRunning()
        {
            return Process.GetProcessesByName(μTorrent.ExecutableBasename).Length > 0;
        }

        private static int? GetPrivateInternetAccessForwardedPort()
        {
            try
            {
                NotificationIcon μTorrentNotificationIcon = NotificationArea.NotificationArea.NotificationIcons.First(
                    notificationIcon => notificationIcon.ProcessName == PrivateInternetAccess.ExecutableBasename);

                Match match = Regex.Match(μTorrentNotificationIcon.ToolTip, μTorrent.TOOLTIP_PATTERN);
                if (match.Success)
                {
                    return int.Parse(match.Groups[1].Value);
                }
                else
                {
                    MessageBox.Show(
                        "Could not read forwarded port from Private Internet Access notification icon tooltip.\r\n\r\n" +
                        "Make sure PIA has \"Request port forwarding\" enabled and is connected to one of the regions that supports port forwarding, like CA Toronto.",
                        "Error setting μTorrent listening port.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // tooltip did not match, possibly disconnected or port forwarding not yet established after recently connecting
                    return null;
                }
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show(
                    "Could not find Private Internet Access icon in the notification area.\r\n\r\nMake sure PIA is running.",
                    "Error setting μTorrent listening port.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // no PIA notification icons found
                return null;
            }
        }

        // not using BEncode.NET because μTorrent rejects settings.dat files encoded by that library
        private static void SetμTorrentListeningPort(int listeningPort)
        {
            const string bindPortStartQuery = ":bind_porti";
            string settingsFile = Path.Combine(μTorrent.InstallationDirectory, μTorrent.SETTINGS_FILENAME);
            byte[] originalBytes = File.ReadAllBytes(settingsFile);

            // remove checksum at beginning of file so μTorrent doesn't reject our changes
            byte[] withFileGuardDeleted = originalBytes.Splice(1, 56).ToArray();
            string withFileGuardDeletedAscii = Encoding.ASCII.GetString(withFileGuardDeleted);

            // change the listening port
            int bindPortStart = withFileGuardDeletedAscii.IndexOf(bindPortStartQuery, StringComparison.Ordinal) +
                                bindPortStartQuery.Length;
            int bindPortEnd = withFileGuardDeletedAscii.IndexOf('e', bindPortStart);
            byte[] portEncoded = Encoding.ASCII.GetBytes(listeningPort.ToString());

            byte[] modifiedBytes = withFileGuardDeleted.Splice(bindPortStart, bindPortEnd - bindPortStart, portEncoded).ToArray();
            File.WriteAllBytes(settingsFile, modifiedBytes);
        }

        private static void LaunchμTorrent(IEnumerable<string> args)
        {
            string arguments = CommandLine.ArgvToCommandLine(args);
            Process.Start(Path.Combine(μTorrent.InstallationDirectory, μTorrent.EXECUTABLE_FILENAME), arguments);
        }
    }
}