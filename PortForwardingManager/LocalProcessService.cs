using System.ComponentModel;
using System.Diagnostics;
using System.IO;

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