using System;

namespace PortForwardingManager.PIA {

    public interface PrivateInternetAccessService {

        /// <summary>
        /// Figure out which port, if any, is being port-forwarded by Private Internet Access.
        /// </summary>
        /// <returns>The public WAN listening port number that is being forwarded to your VPN client</returns>
        /// <exception cref="PrivateInternetAccessException.UnknownForwardedPort">If PIA is running but not forwarding any
        /// ports (possibly because it's still starting up, disconnected, or the logs have already rotated since connecting).</exception>
        /// <exception cref="PrivateInternetAccessException.NoDaemonLogFile">If the daemon debug log file does not
        /// exist because PIA debug logging is disabled or the installation directory is incorrect.</exception>
        /// <exception cref="PrivateInternetAccessException.PortForwardingDisabled">If PIA has port forwarding disabled, or is
        /// connected to a server that does not allow port forwarding.</exception>
        ushort GetPrivateInternetAccessForwardedPort();

    }

    public class PrivateInternetAccessException: Exception {

        private PrivateInternetAccessException() { }

        public class UnknownForwardedPort: PrivateInternetAccessException { }

        public class NoDaemonLogFile: PrivateInternetAccessException { }

        public class PortForwardingDisabled: PrivateInternetAccessException { }

    }

}