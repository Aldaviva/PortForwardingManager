using System.Windows.Forms;

namespace PortForwardingManager
{
    public interface ErrorReportingService
    {
        void ReportError(string title, string description);
    }

    public class ErrorReportingServiceImpl : ErrorReportingService
    {
        public void ReportError(string title, string description)
        {
            MessageBox.Show(description, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}