using System;
using FakeItEasy;
using FluentAssertions;
using NotificationArea;
using PortForwardingManager.PIA;
using Xunit;

namespace Test
{
    public class PrivateInternetAccessServiceTest
    {
        private PrivateInternetAccessServiceImpl service = new PrivateInternetAccessServiceImpl();
        private NotificationArea.NotificationArea notificationArea = A.Fake<NotificationArea.NotificationArea>();

        public PrivateInternetAccessServiceTest()
        {
            service.NotificationArea = notificationArea;
        }

        [Fact]
        public void GetPrivateInternetAccessForwardedPortSuccess()
        {
            A.CallTo(() => notificationArea.NotificationIcons).Returns(new[]
            {
                new NotificationIcon(@"ESET NOD32 Antivirus™ 11.1.54.0", 8536, "egui"),
                new NotificationIcon(@"Private Internet Access - You are connected (UK Manchester) - (196.57.90.208) [ Port: 1591 ]",
                    4700, "pia_nw")
            });

            service.GetPrivateInternetAccessForwardedPort().Should().Be(1591);
        }

        [Fact]
        public void GetPrivateInternetAccessForwardedPortNotForwarding()
        {
            A.CallTo(() => notificationArea.NotificationIcons).Returns(new[]
            {
                new NotificationIcon(@"ESET NOD32 Antivirus™ 11.1.54.0", 8536, "egui"),
                new NotificationIcon(@"Private Internet Access - You are connected (UK Manchester) - (196.57.90.208)",
                    4700, "pia_nw")
            });

            Action thrower = () => service.GetPrivateInternetAccessForwardedPort();

            thrower.Should().Throw<PrivateInternetAccessException.NoForwardedPort>();
        }

        [Fact]
        public void GetPrivateInternetAccessForwardedPortNoNotificationIcon()
        {
            A.CallTo(() => notificationArea.NotificationIcons).Returns(new[]
            {
                new NotificationIcon(@"ESET NOD32 Antivirus™ 11.1.54.0", 8536, "egui")
            });

            Action thrower = () => service.GetPrivateInternetAccessForwardedPort();

            thrower.Should().Throw<PrivateInternetAccessException.NoNotificationIcon>();
        }
    }
}