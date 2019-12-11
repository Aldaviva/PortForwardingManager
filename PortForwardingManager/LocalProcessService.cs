using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace PortForwardingManager
{
    public interface LocalProcessService
    {
        Process[] getProcessesByName(string processName);
        Process start(string fileName, string arguments);
    }

    public class LocalProcessServiceImpl : LocalProcessService
    {
        public Process[] getProcessesByName(string processName)
        {
            return Process.GetProcessesByName(processName);
        }

        public Process start(string fileName, string arguments)
        {
            try
            {
                return Process.Start(fileName, arguments);
            }
            catch (Win32Exception e)
            {
                if (e.ErrorCode == -2147467259)
                {
                    throw new FileNotFoundException("Could not start process", fileName, e);
                }

                throw;
            }
        }
    }
}