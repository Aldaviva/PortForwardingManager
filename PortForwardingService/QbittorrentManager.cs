#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using PortForwardingService.ListeningPortEditors;

namespace PortForwardingService {

    public class QbittorrentManager {

        private readonly ListeningPortEditor webApiListeningPortEditor = new WebApiListeningPortEditor();
        private readonly ListeningPortEditor configurationFileListeningPortEditor = new ConfigurationFileListeningPortEditor();

        private readonly string qBittorrentExecutablePath =
            Environment.ExpandEnvironmentVariables(@"%programfiles%\qBittorrent\qbittorrent.exe");

        public ushort? getQbittorrentConfigurationListeningPort() => configurationFileListeningPortEditor.getListeningPort();

        public void setQbittorrentListeningPort(ushort listeningPort) {
            ListeningPortEditor listeningPortEditor = isQbittorrentRunning()
                ? webApiListeningPortEditor
                : configurationFileListeningPortEditor;

            listeningPortEditor.setListeningPort(listeningPort);
        }

        private bool isQbittorrentRunning() {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(qBittorrentExecutablePath)).Length > 0;
        }

    }

}