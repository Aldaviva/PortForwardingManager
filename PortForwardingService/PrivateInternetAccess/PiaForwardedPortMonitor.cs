#nullable enable

using KoKo.Property;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PortForwardingService.PrivateInternetAccess;

public class PiaForwardedPortMonitor: IDisposable {

    public Property<ushort?> forwardedPort => piaForwardedPort;
    private readonly StoredProperty<ushort?> piaForwardedPort = new();

    private Process? piaMonitorProcess;

    private volatile bool isShutDown;

    public void listenForPiaPortForwardChanges() {
        Task.Run(() => {
            isShutDown = false;

            string piaCtlPath = Environment.ExpandEnvironmentVariables(@"%programfiles%\Private Internet Access\piactl.exe");

            ProcessStartInfo startInfo = new(piaCtlPath) {
                Arguments              = "monitor portforward",
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            piaMonitorProcess = Process.Start(startInfo);

            if (piaMonitorProcess == null) {
                throw new PrivateInternetAccessException.UnknownForwardedPort();
            }

            piaMonitorProcess.OutputDataReceived += onPiaMonitorOutput;

            piaMonitorProcess.BeginOutputReadLine(); //required for OutputDataReceived events to be emitted

            piaMonitorProcess.Exited += (_, _) => {
                if (!isShutDown) {
                    listenForPiaPortForwardChanges();
                }
            };
        });
    }

    private void onPiaMonitorOutput(object sender, DataReceivedEventArgs args) {
        try {
            switch (args.Data) {
                case "Inactive":
                case "Attempting":
                case "Unavailable":
                    throw new PrivateInternetAccessException.PortForwardingDisabled();
                case "Failed":
                    throw new PrivateInternetAccessException.UnknownForwardedPort();
                default:
                    try {
                        piaForwardedPort.Value = Convert.ToUInt16(args.Data);
                    } catch (FormatException) {
                        throw new PrivateInternetAccessException.UnknownForwardedPort();
                    } catch (OverflowException) {
                        throw new PrivateInternetAccessException.UnknownForwardedPort();
                    }

                    break;
            }
        } catch (PrivateInternetAccessException) {
            piaForwardedPort.Value = null;
        }
    }

    public void Dispose() {
        isShutDown = true;
        piaMonitorProcess?.Kill();
        piaMonitorProcess?.Dispose();
        piaMonitorProcess = null;
    }

}