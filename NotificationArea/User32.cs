using System;
using System.Runtime.InteropServices;

namespace NotificationArea
{
    internal static class Toolbar
    {
        public const uint GETBUTTON = WindowMessage.USER + 23;
        public const uint BUTTONCOUNT = WindowMessage.USER + 24;
        public const uint GETBUTTONTEXTW = WindowMessage.USER + 75;
    }

    internal static class WindowMessage
    {
        public const uint USER = 0x0400; // 0x0400 - 0x7FFF
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TrayIcon
    {
        private readonly int iBitmap;
        public readonly int idCommand;
        private readonly byte fsState;
        private readonly byte fsStyle;
        private readonly byte bReserved1;
        private readonly byte bReserved2;
        public readonly ulong dwData;
        private readonly IntPtr iString;
    }

    internal static class User32
    {
        private const string USER32 = "user32.dll";

        [DllImport(USER32)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport(USER32)]
        public static extern uint SendMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);

        [DllImport(USER32, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport(USER32, CharSet = CharSet.Auto)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport(USER32, SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}