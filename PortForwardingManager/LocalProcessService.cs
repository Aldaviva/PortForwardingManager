using System.Diagnostics;

namespace PortForwardingManager
{
    public interface LocalProcessService
    {
        Process[] GetProcessesByName(string processName);
        Process Start(string fileName, string arguments);
    }

    public class LocalProcessServiceImpl : LocalProcessService
    {
        public Process[] GetProcessesByName(string processName)
        {
            return Process.GetProcessesByName(processName);
        }

        public Process Start(string fileName, string arguments)
        {
            return Process.Start(fileName, arguments);
        }
    }
}