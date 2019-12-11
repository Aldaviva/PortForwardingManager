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

            A.CallTo(() => μTorrentData.installationDirectory).CallsBaseMethod();
        }

        [Fact]
        public void isμTorrentAlreadyRunningTrue()
        {
            A.CallTo(() => localProcessService.getProcessesByName("uTorrent")).Returns(new[] { new Process() });

            service.isμTorrentAlreadyRunning().Should().BeTrue();
        }

        [Fact]
        public void isμTorrentAlreadyRunningFalse()
        {
            A.CallTo(() => localProcessService.getProcessesByName("uTorrent")).Returns(new Process[0]);

            service.isμTorrentAlreadyRunning().Should().BeFalse();
        }

        [Fact]
        public void launchμTorrentNoArgs()
        {
            service.launchμTorrent(Enumerable.Empty<string>());

            string expectedFileName = Environment.ExpandEnvironmentVariables(@"C:\Program Files (x86)\uTorrent\uTorrent.exe");
            A.CallTo(() => localProcessService.start(expectedFileName, "")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void launchμTorrentWithMagnetUri()
        {
            service.launchμTorrent(new []{ @"magnet:?xt=urn:btih:OQY2S2NTI7QUXOTEDM2RPQBE662A3637" });

            string expectedFileName = Environment.ExpandEnvironmentVariables(@"C:\Program Files (x86)\uTorrent\uTorrent.exe");
            A.CallTo(() => localProcessService.start(expectedFileName, @"""magnet:?xt=urn:btih:OQY2S2NTI7QUXOTEDM2RPQBE662A3637""")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void setμTorrentListeningPort()
        {
            A.CallTo(() => μTorrentData.installationDirectory).Returns(@"Data");

            File.Delete(@"Data\settings.dat");
            File.Copy(@"Data\settings-before.dat", @"Data\settings.dat");

            service.setμTorrentListeningPort(6789);

            byte[] actual = File.ReadAllBytes(@"Data\settings.dat");
            byte[] expected = File.ReadAllBytes(@"Data\settings-expected.dat");
            actual.Should().Equal(expected);

//            File.Delete(@"Data\settings.dat");
        }

        [Fact]
        public void doNothingIfSettingsAreAlreadyCorrect()
        {
            A.CallTo(() => μTorrentData.installationDirectory).Returns(@"Data");

            File.Delete(@"Data\settings.dat");
            File.Copy(@"Data\settings-expected.dat", @"Data\settings.dat");

            DateTime lastModified = File.GetLastWriteTimeUtc(@"Data\settings.dat");

            service.setμTorrentListeningPort(6789);

            File.GetLastWriteTimeUtc(@"Data\settings.dat").Should()
                .Be(lastModified, "file should not have been modified by service");
        }
    }
}