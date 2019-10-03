﻿using System.Collections.Generic;
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
        public void DoNotUpdateSettingsWhenAlreadyRunning()
        {
            A.CallTo(() => μTorrent.IsμTorrentAlreadyRunning()).Returns(true);

            launcher.UpdateSettingsAndLaunch(Enumerable.Empty<string>());

            A.CallTo(() => μTorrent.SetμTorrentListeningPort(A<ushort>._)).MustNotHaveHappened();
            A.CallTo(() => μTorrent.LaunchμTorrent(A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DoNotUpdateSettingsWhenPiaNotPortFowarding()
        {
            A.CallTo(() => μTorrent.IsμTorrentAlreadyRunning()).Returns(false);
            A.CallTo(() => pia.GetPrivateInternetAccessForwardedPort()).Throws<PrivateInternetAccessException.UnknownForwardedPort>();

            launcher.UpdateSettingsAndLaunch(Enumerable.Empty<string>());

            A.CallTo(() => μTorrent.SetμTorrentListeningPort(A<ushort>._)).MustNotHaveHappened();
            A.CallTo(() => μTorrent.LaunchμTorrent(A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => errorReportingService.ReportError(A<string>._, A<string>._, A<MessageBoxIcon>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DoNotUpdateSettingsWhenNoPiaDaemonLogFile()
        {
            A.CallTo(() => μTorrent.IsμTorrentAlreadyRunning()).Returns(false);
            A.CallTo(() => pia.GetPrivateInternetAccessForwardedPort()).Throws<PrivateInternetAccessException.NoDaemonLogFile>();

            launcher.UpdateSettingsAndLaunch(Enumerable.Empty<string>());

            A.CallTo(() => μTorrent.SetμTorrentListeningPort(A<ushort>._)).MustNotHaveHappened();
            A.CallTo(() => μTorrent.LaunchμTorrent(A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => errorReportingService.ReportError(A<string>._, A<string>._, A<MessageBoxIcon>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CannotFindμTorrent()
        {
            A.CallTo(() => μTorrent.IsμTorrentAlreadyRunning()).Returns(false);
            A.CallTo(() => pia.GetPrivateInternetAccessForwardedPort()).Returns((ushort) 12345);
            A.CallTo(() => μTorrent.LaunchμTorrent(A<IEnumerable<string>>._)).Throws<FileNotFoundException>();

            launcher.UpdateSettingsAndLaunch(Enumerable.Empty<string>());

            A.CallTo(() => μTorrent.SetμTorrentListeningPort(A<ushort>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => μTorrent.LaunchμTorrent(A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => errorReportingService.ReportError(A<string>._, A<string>._, A<MessageBoxIcon>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void SetListeningPort()
        {
            A.CallTo(() => μTorrent.IsμTorrentAlreadyRunning()).Returns(false);
            A.CallTo(() => pia.GetPrivateInternetAccessForwardedPort()).Returns((ushort) 12345);

            launcher.UpdateSettingsAndLaunch(Enumerable.Empty<string>());

            A.CallTo(() => μTorrent.SetμTorrentListeningPort(12345)).MustHaveHappenedOnceExactly();
            A.CallTo(() => μTorrent.LaunchμTorrent(A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
        }
    }
}