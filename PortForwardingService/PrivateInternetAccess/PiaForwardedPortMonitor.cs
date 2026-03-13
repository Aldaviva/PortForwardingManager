#nullable enable

using KoKo.Property;
using NLog;
using PortForwardingService.qBittorrent;
using System.Diagnostics;
using Unfucked;
using WindowsFirewallHelper;

namespace PortForwardingService.PrivateInternetAccess;

public class PiaForwardedPortMonitor: IDisposable {

    private static readonly Logger LOGGER = LogManager.GetLogger(typeof(PiaForwardedPortMonitor).FullName!);

    private static readonly string PiaCtlPath = Environment.ExpandEnvironmentVariables(@"%programfiles%\Private Internet Access\piactl.exe");

    public Property<ushort?> forwardedPort => piaForwardedPort;
    private readonly StoredProperty<ushort?> piaForwardedPort = new();

    private readonly SemaphoreSlim stdoutReaderLock = new(1);

    private Process? piaMonitorProcess;

    private volatile bool isShutDown;

    public void listenForPiaPortForwardChanges() {
        Task.Run(() => {
            isShutDown = false;

            ProcessStartInfo startInfo = new(PiaCtlPath) {
                Arguments              = "monitor portforward",
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            piaMonitorProcess = Process.Start(startInfo);

            if (piaMonitorProcess == null) {
                throw new PrivateInternetAccessException.UnknownForwardedPort();
            }

            piaMonitorProcess.EnableRaisingEvents =  true;
            piaMonitorProcess.OutputDataReceived  += onPiaMonitorOutput;

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
            piaForwardedPort.Value = parseForwardedPort(args.Data);
        } catch (PrivateInternetAccessException.PortForwardingFailed) {
            LOGGER.Warn("PIA port forwarding failed, reconnecting PIA to try to fix it.");
            piaForwardedPort.Value = null;
            Task.Run(async () => await reconnectPia());
        } catch (PrivateInternetAccessException) {
            piaForwardedPort.Value = null;
        }
    }

    /// <exception cref="PrivateInternetAccessException"></exception>
    private static ushort parseForwardedPort(string rawValue) {
        switch (rawValue) {
            case "Inactive":
            case "Attempting":
            case "Unavailable":
                throw new PrivateInternetAccessException.PortForwardingDisabled();
            case "Failed":
                throw new PrivateInternetAccessException.PortForwardingFailed();
            default:
                try {
                    return Convert.ToUInt16(rawValue);
                } catch (FormatException) {
                    throw new PrivateInternetAccessException.UnknownForwardedPort();
                } catch (OverflowException) {
                    throw new PrivateInternetAccessException.UnknownForwardedPort();
                }
        }
    }

    private static async Task reconnectPia() {
        if (QbittorrentManager.findExecutableAbsoluteFilename() is not {} executableAbsoluteFilename) return;

        const string           RULE_NAME    = "Block qBittorrent while reconnecting PIA";
        const FirewallProfiles PROFILES     = FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public;
        IFirewallRule          inboundRule  = FirewallManager.Instance.CreateApplicationRule(PROFILES, RULE_NAME, FirewallAction.Block, executableAbsoluteFilename);
        IFirewallRule          outboundRule = FirewallManager.Instance.CreateApplicationRule(PROFILES, RULE_NAME, FirewallAction.Block, executableAbsoluteFilename);
        inboundRule.Direction  = FirewallDirection.Inbound;
        outboundRule.Direction = FirewallDirection.Outbound;
        FirewallManager.Instance.Rules.Add(inboundRule);
        FirewallManager.Instance.Rules.Add(outboundRule);

        await Task.Delay(TimeSpan.FromSeconds(10));

        if ((await Processes.ExecFile(PiaCtlPath, "connect")).ExitCode != 0) return;

        await Task.Delay(TimeSpan.FromSeconds(10));

        ProcessResult getPortForwardProcess = await Processes.ExecFile(PiaCtlPath, "get portforward");
        if (getPortForwardProcess.ExitCode != 0) return;
        try {
            parseForwardedPort(getPortForwardProcess.StdOut);
        } catch (PrivateInternetAccessException) {
            return;
        }

        FirewallManager.Instance.Rules.Remove(inboundRule);
        FirewallManager.Instance.Rules.Remove(outboundRule);
    }

    public void Dispose() {
        isShutDown = true;
        piaMonitorProcess?.Kill();
        piaMonitorProcess?.Dispose();
        piaMonitorProcess = null;
        stdoutReaderLock.Dispose();
    }

}