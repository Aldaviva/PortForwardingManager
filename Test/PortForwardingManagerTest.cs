using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FakeItEasy;
using PortForwardingManager;
using PortForwardingManager.PIA;
using PortForwardingManager.μTorrent;
using Xunit;

namespace Test
{
    public class PortForwardingManagerTest
    {
        private readonly PortForwardingManager.PortForwardingManager launcher = new PortForwardingManager.PortForwardingManager();
        private readonly μTorrentService μTorrent = A.Fake<μTorrentService>();
        private readonly PrivateInternetAccessService pia = A.Fake<PrivateInternetAccessService>();
        private readonly ErrorReportingService errorReportingService = A.Fake<ErrorReportingService>();

        public PortForwardingManagerTest()
        {
            launcher.μTorrentService = μTorrent;
            launcher.PrivateInternetAccessService = pia;
            launcher.ErrorReportingService = errorReportingService;
        }

        [Fact]
        public void doNotUpdateSettingsWhenAlreadyRunning()
        {
            A.CallTo(() => μTorrent.isμTorrentAlreadyRunning()).Returns(true);

            launcher.updateSettingsAndLaunch(Enumerable.Empty<string>());

            A.CallTo(() => μTorrent.setμTorrentListeningPort(A<ushort>._)).MustNotHaveHappened();
            A.CallTo(() => μTorrent.launchμTorrent(A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void doNotUpdateSettingsWhenPiaNotPortFowarding()
        {
            A.CallTo(() => μTorrent.isμTorrentAlreadyRunning()).Returns(false);
            A.CallTo(() => pia.getPrivateInternetAccessForwardedPort()).Throws<PrivateInternetAccessException.UnknownForwardedPort>();

            launcher.updateSettingsAndLaunch(Enumerable.Empty<string>());

            A.CallTo(() => μTorrent.setμTorrentListeningPort(A<ushort>._)).MustNotHaveHappened();
            A.CallTo(() => μTorrent.launchμTorrent(A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => errorReportingService.reportError(A<string>._, A<string>._, A<MessageBoxIcon>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void doNotUpdateSettingsWhenNoPiaDaemonLogFile()
        {
            A.CallTo(() => μTorrent.isμTorrentAlreadyRunning()).Returns(false);
            A.CallTo(() => pia.getPrivateInternetAccessForwardedPort()).Throws<PrivateInternetAccessException.NoDaemonLogFile>();

            launcher.updateSettingsAndLaunch(Enumerable.Empty<string>());

            A.CallTo(() => μTorrent.setμTorrentListeningPort(A<ushort>._)).MustNotHaveHappened();
            A.CallTo(() => μTorrent.launchμTorrent(A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => errorReportingService.reportError(A<string>._, A<string>._, A<MessageBoxIcon>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void cannotFindμTorrent()
        {
            A.CallTo(() => μTorrent.isμTorrentAlreadyRunning()).Returns(false);
            A.CallTo(() => pia.getPrivateInternetAccessForwardedPort()).Returns((ushort) 12345);
            A.CallTo(() => μTorrent.launchμTorrent(A<IEnumerable<string>>._)).Throws<FileNotFoundException>();

            launcher.updateSettingsAndLaunch(Enumerable.Empty<string>());

            A.CallTo(() => μTorrent.setμTorrentListeningPort(A<ushort>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => μTorrent.launchμTorrent(A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => errorReportingService.reportError(A<string>._, A<string>._, A<MessageBoxIcon>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void setListeningPort()
        {
            A.CallTo(() => μTorrent.isμTorrentAlreadyRunning()).Returns(false);
            A.CallTo(() => pia.getPrivateInternetAccessForwardedPort()).Returns((ushort) 12345);

            launcher.updateSettingsAndLaunch(Enumerable.Empty<string>());

            A.CallTo(() => μTorrent.setμTorrentListeningPort(12345)).MustHaveHappenedOnceExactly();
            A.CallTo(() => μTorrent.launchμTorrent(A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
        }
    }
}