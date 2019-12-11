using System;
using FluentAssertions;
using PortForwardingManager.μTorrent;
using Xunit;

namespace Test
{
    public class μTorrentDataTest
    {
        private readonly μTorrentData μTorrentData = new μTorrentData();

        [Fact]
        public void executableBasename()
        {
            μTorrentData.executableBasename.Should().Be("uTorrent");
        }

        [Fact]
        public void installationDirectory()
        {
            μTorrentData.installationDirectory.Should()
                .Be(Environment.ExpandEnvironmentVariables(@"C:\Program Files (x86)\uTorrent\"));
        }
    }
}