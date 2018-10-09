using FluentAssertions;
using PortForwardingManager.PIA;
using Xunit;

namespace Test
{
    public class PrivateInternetAccessDataTest
    {
        [Fact]
        public void ExecutableBasename()
        {
            PrivateInternetAccessData.ExecutableBasename.Should().Be("pia_nw");
        }
    }
}