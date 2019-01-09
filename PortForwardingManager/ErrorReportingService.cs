using System.Windows.Forms;

namespace PortForwardingManager
{
    public interface ErrorReportingService
    {
        void ReportError(string title, string description, MessageBoxIcon icon);
    }

    public class ErrorReportingServiceImpl : ErrorReportingService
    {
        public void ReportError(string title, string description, MessageBoxIcon icon)
        {
            MessageBox.Show(description, title, MessageBoxButtons.OK, icon);
        }
    }
}