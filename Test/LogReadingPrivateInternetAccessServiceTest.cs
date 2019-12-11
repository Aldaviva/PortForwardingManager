using System;
using FluentAssertions;
using PortForwardingManager.PIA;
using Xunit;

namespace Test
{
    public class LogReadingPrivateInternetAccessServiceTest
    {
        private readonly LogReadingPrivateInternetAccessServiceImpl service = new LogReadingPrivateInternetAccessServiceImpl();

        [Fact(Skip = "deprecated")]
        public void getPrivateInternetAccessForwardedPortSuccess()
        {
            PrivateInternetAccessData.InstallationDirectory = @"Data\forwarding\";

            service.getPrivateInternetAccessForwardedPort().Should().Be(54473);
        }

        [Fact(Skip = "deprecated")]
        public void getPrivateInternetAccessForwardedPortNotForwarding()
        {
            PrivateInternetAccessData.InstallationDirectory = @"Data\not-forwarding\";

            Action thrower = () => service.getPrivateInternetAccessForwardedPort();
            
            thrower.Should().Throw<PrivateInternetAccessException.UnknownForwardedPort>();
        }

        [Fact(Skip = "deprecated")]
        public void getPrivateInternetAccessForwardedPortNeverStartedForwarding()
        {
            PrivateInternetAccessData.InstallationDirectory = @"Data\never-started-forwarding\";

            Action thrower = () => service.getPrivateInternetAccessForwardedPort();
            
            thrower.Should().Throw<PrivateInternetAccessException.UnknownForwardedPort>();
        }

        [Fact(Skip = "deprecated")]
        public void getPrivateInternetAccessForwardedPortNoDaemonLogFile()
        {
            PrivateInternetAccessData.InstallationDirectory = @"Data\debug-disabled\";

            Action thrower = () => service.getPrivateInternetAccessForwardedPort();

            thrower.Should().Throw<PrivateInternetAccessException.NoDaemonLogFile>();
        }
    }
}