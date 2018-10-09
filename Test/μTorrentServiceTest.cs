using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using PortForwardingManager;
using PortForwardingManager.μTorrent;
using Xunit;

namespace Test
{
    public class μTorrentServiceTest
    {
        private readonly μTorrentServiceImpl service = new μTorrentServiceImpl();
        private readonly LocalProcessService localProcessService = A.Fake<LocalProcessService>();
        private readonly μTorrentData μTorrentData = A.Fake<μTorrentData>();

        public μTorrentServiceTest()
        {
            service.LocalProcessService = localProcessService;
            service.μTorrentData = μTorrentData;

            A.CallTo(() => μTorrentData.InstallationDirectory).CallsBaseMethod();
        }

        [Fact]
        public void IsμTorrentAlreadyRunningTrue()
        {
            A.CallTo(() => localProcessService.GetProcessesByName("uTorrent")).Returns(new[] { new Process() });

            service.IsμTorrentAlreadyRunning().Should().BeTrue();
        }

        [Fact]
        public void IsμTorrentAlreadyRunningFalse()
        {
            A.CallTo(() => localProcessService.GetProcessesByName("uTorrent")).Returns(new Process[0]);

            service.IsμTorrentAlreadyRunning().Should().BeFalse();
        }

        [Fact]
        public void LaunchμTorrentNoArgs()
        {
            service.LaunchμTorrent(Enumerable.Empty<string>());

            string expectedFileName = Environment.ExpandEnvironmentVariables(@"C:\Program Files (x86)\uTorrent\uTorrent.exe");
            A.CallTo(() => localProcessService.Start(expectedFileName, "")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void LaunchμTorrentWithMagnetUri()
        {
            service.LaunchμTorrent(new []{ @"magnet:?xt=urn:btih:OQY2S2NTI7QUXOTEDM2RPQBE662A3637" });

            string expectedFileName = Environment.ExpandEnvironmentVariables(@"C:\Program Files (x86)\uTorrent\uTorrent.exe");
            A.CallTo(() => localProcessService.Start(expectedFileName, @"""magnet:?xt=urn:btih:OQY2S2NTI7QUXOTEDM2RPQBE662A3637""")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void SetμTorrentListeningPort()
        {
            A.CallTo(() => μTorrentData.InstallationDirectory).Returns(@"Data");

            File.Delete(@"Data\settings.dat");
            File.Copy(@"Data\settings-before.dat", @"Data\settings.dat");

            service.SetμTorrentListeningPort(6789);

            byte[] actual = File.ReadAllBytes(@"Data\settings.dat");
            byte[] expected = File.ReadAllBytes(@"Data\settings-expected.dat");
            actual.Should().Equal(expected);

//            File.Delete(@"Data\settings.dat");
        }

        [Fact]
        public void DoNothingIfSettingsAreAlreadyCorrect()
        {
            A.CallTo(() => μTorrentData.InstallationDirectory).Returns(@"Data");

            File.Delete(@"Data\settings.dat");
            File.Copy(@"Data\settings-expected.dat", @"Data\settings.dat");

            DateTime lastModified = File.GetLastWriteTimeUtc(@"Data\settings.dat");

            service.SetμTorrentListeningPort(6789);

            File.GetLastWriteTimeUtc(@"Data\settings.dat").Should()
                .Be(lastModified, "file should not have been modified by service");
        }
    }
}