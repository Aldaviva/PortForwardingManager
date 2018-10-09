using System;
using FluentAssertions;
using PortForwardingManager.μTorrent;
using Xunit;

namespace Test
{
    public class μTorrentDataTest
    {
        private μTorrentData μTorrentData = new μTorrentData();

        [Fact]
        public void ExecutableBasename()
        {
            μTorrentData.ExecutableBasename.Should().Be("uTorrent");
        }

        [Fact]
        public void InstallationDirectory()
        {
            μTorrentData.InstallationDirectory.Should()
                .Be(Environment.ExpandEnvironmentVariables(@"C:\Program Files (x86)\uTorrent\"));
        }
    }
}