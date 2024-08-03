#nullable enable

using System.Threading.Tasks;

namespace PortForwardingService.qBittorrent.ListeningPortEditors;

internal interface ListeningPortEditor {

    Task setListeningPort(ushort listeningPort);

    Task<ushort?> getListeningPort();

}