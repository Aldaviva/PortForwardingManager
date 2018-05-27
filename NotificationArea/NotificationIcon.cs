namespace NotificationArea
{
    public class NotificationIcon
    {
        public string ToolTip { get; }
        public int ProcessId { get; }
        public string ProcessName { get; }

        public NotificationIcon(string toolTip, int processId, string processName)
        {
            ToolTip = toolTip;
            ProcessId = processId;
            ProcessName = processName;
        }
    }
}