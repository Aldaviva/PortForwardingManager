#nullable enable

using System.Threading.Tasks;

namespace PortForwardingService.ListeningPortEditors {

    internal interface ListeningPortEditor {

        Task setListeningPort(ushort listeningPort);

        ushort? getListeningPort();

    }

}