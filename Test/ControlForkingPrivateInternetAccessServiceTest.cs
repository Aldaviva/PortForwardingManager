using System.Diagnostics;
using System.IO;
using System.Text;
using FakeItEasy;
using FluentAssertions;
using PortForwardingManager.PIA;
using Smocks;
using Smocks.Matching;
using Xunit;

namespace Test {

    public class ControlForkingPrivateInternetAccessServiceTest {

        [Fact]
        public void enabled() {
#if !DEBUG
    Assert.True(false, "Smocks do not work in Release mode. Please change the build to Debug mode and run the test again.");
#endif

            Smock.Run(context => {
                ProcessStartInfo actualProcessStartInfo = null;

                var mockProcess = A.Fake<Process>();
                context.Setup(() => Process.Start(It.IsAny<ProcessStartInfo>()))
                    .Callback<ProcessStartInfo>(info => actualProcessStartInfo = info)
                    .Returns(mockProcess);

                var stdOut = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("12345\r\n")));
                context.Setup(() => mockProcess.StandardOutput)
                    .Returns(stdOut);

                ushort actual = new ControlForkingPrivateInternetAccessService().getPrivateInternetAccessForwardedPort();
                actual.Should().Be(12345);

                context.Verify();
                actualProcessStartInfo.FileName.Should().Be(@"C:\Program Files\Private Internet Access\piactl.exe");
                actualProcessStartInfo.Arguments.Should().Be("get portforward");
                actualProcessStartInfo.RedirectStandardOutput.Should().Be(true);
                actualProcessStartInfo.UseShellExecute.Should().Be(false);
                actualProcessStartInfo.CreateNoWindow.Should().Be(true);
            });
        }

    }

}