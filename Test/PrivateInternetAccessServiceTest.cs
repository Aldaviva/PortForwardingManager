using System;
using FluentAssertions;
using PortForwardingManager.PIA;
using Xunit;

namespace Test
{
    public class PrivateInternetAccessServiceTest
    {
        private readonly PrivateInternetAccessServiceImpl service = new PrivateInternetAccessServiceImpl();

        [Fact]
        public void GetPrivateInternetAccessForwardedPortSuccess()
        {
            PrivateInternetAccessData.InstallationDirectory = @"Data\forwarding\";

            service.GetPrivateInternetAccessForwardedPort().Should().Be(54473);
        }

        [Fact]
        public void GetPrivateInternetAccessForwardedPortNotForwarding()
        {
            PrivateInternetAccessData.InstallationDirectory = @"Data\not-forwarding\";

            Action thrower = () => service.GetPrivateInternetAccessForwardedPort();
            
            thrower.Should().Throw<PrivateInternetAccessException.NoForwardedPort>();
        }
        [Fact]
        public void GetPrivateInternetAccessForwardedPortNeverStartedForwarding()
        {
            PrivateInternetAccessData.InstallationDirectory = @"Data\never-started-forwarding\";

            Action thrower = () => service.GetPrivateInternetAccessForwardedPort();
            
            thrower.Should().Throw<PrivateInternetAccessException.NoForwardedPort>();
        }

        [Fact]
        public void GetPrivateInternetAccessForwardedPortNoDaemonLogFile()
        {
            PrivateInternetAccessData.InstallationDirectory = @"Data\debug-disabled\";

            Action thrower = () => service.GetPrivateInternetAccessForwardedPort();

            thrower.Should().Throw<PrivateInternetAccessException.NoDaemonLogFile>();
        }
    }
}