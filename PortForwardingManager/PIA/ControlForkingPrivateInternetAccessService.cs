using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace PortForwardingManager.PIA {

    /// <summary>
    /// #2: Read the PIA forwarded port by running 'piactl.exe get forwardedport'. This command-line program was added in PIA v1.6.
    /// The return values are not documented. I have seen a bare port number, and the strings "Inactive" or "Attempting", but there may
    /// be other possible values printed to standard output. This command works even if PIA is not running.
    ///
    /// There are multiple reasons for "Inactive" to be returned: PIA may not be running, it may be running but disconnected from VPN,
    /// it may be connected to VPN but not connected to a VPN server capable of port fowarding, or it may be connected to a VPN server
    /// capable of port forwarding but port forwarding may be disabled in the client settings.
    /// </summary>
    public class ControlForkingPrivateInternetAccessService: PrivateInternetAccessService {

        private static string piaCtlPath => Path.Combine(PrivateInternetAccessData.InstallationDirectory, "piactl.exe");

        public ushort getPrivateInternetAccessForwardedPort() {
            Process piaCtlProcess = Process.Start(new ProcessStartInfo(piaCtlPath, "get portforward") {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (piaCtlProcess == null) {
                throw new PrivateInternetAccessException.UnknownForwardedPort();
            }

            string stdOutLine = piaCtlProcess.StandardOutput.ReadLine();

            switch (stdOutLine) {
                case "Inactive":
                case "Attempting":
                    throw new PrivateInternetAccessException.PortForwardingDisabled();
                case "Error":
                    throw new PrivateInternetAccessException.UnknownForwardedPort();
                default:
                    try {
                        return Convert.ToUInt16(stdOutLine);
                    } catch (FormatException) {
                        throw new PrivateInternetAccessException.UnknownForwardedPort();
                    } catch (OverflowException) {
                        throw new PrivateInternetAccessException.UnknownForwardedPort();
                    }
            }

        }

    }

}