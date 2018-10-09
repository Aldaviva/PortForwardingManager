using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NotificationArea
{
    public interface NotificationArea
    {
        IEnumerable<NotificationIcon> NotificationIcons { get; }
    }

    /// <summary>
    /// From https://stackoverflow.com/questions/33652756/how-to-get-the-processes-that-have-systray-icon
    /// </summary>
    public class NotificationAreaImpl: NotificationArea
    {
        public IEnumerable<NotificationIcon> NotificationIcons => FindProcessInSystray();

        private static IEnumerable<NotificationIcon> FindProcessInSystray()
        {
            IntPtr systemTrayHandle = GetSystemTrayHandle();

            uint trayIconCount = User32.SendMessage(systemTrayHandle, Toolbar.BUTTONCOUNT, 0, 0);

            var result = new List<NotificationIcon>();

            for (int trayIconIndex = 0; trayIconIndex < trayIconCount; trayIconIndex++)
            {
                var tbButton = new TrayIcon();
                string text = string.Empty;
                IntPtr ipWindowHandle = IntPtr.Zero;
                uint pid = 0;

                bool b = GetTrayIcon(systemTrayHandle, trayIconIndex, ref tbButton, ref text, ref ipWindowHandle, ref pid);
                if (b && tbButton.dwData != 0)
                {
                    result.Add(new NotificationIcon(text, (int) pid, Process.GetProcessById((int) pid).ProcessName));
                }
            }

            return result;
        }

        private static IntPtr GetSystemTrayHandle()
        {
            IntPtr hWndTray = User32.FindWindow("Shell_TrayWnd", null);
            if (hWndTray != IntPtr.Zero)
            {
                hWndTray = User32.FindWindowEx(hWndTray, IntPtr.Zero, "TrayNotifyWnd", null);
                if (hWndTray != IntPtr.Zero)
                {
                    hWndTray = User32.FindWindowEx(hWndTray, IntPtr.Zero, "SysPager", null);
                    if (hWndTray != IntPtr.Zero)
                    {
                        hWndTray = User32.FindWindowEx(hWndTray, IntPtr.Zero, "ToolbarWindow32", null);
                        return hWndTray;
                    }
                }
            }

            return IntPtr.Zero;
        }


        [SuppressMessage("ReSharper", "RedundantAssignment")]
        private static unsafe bool GetTrayIcon(IntPtr hToolbar, int i, ref TrayIcon trayIcon, ref string text,
            ref IntPtr ipWindowHandle, ref uint pid)
        {
            const int bufferSize = 0x1000;

            var localBuffer = new byte[bufferSize];

            User32.GetWindowThreadProcessId(hToolbar, out uint explorerProcessId);

            IntPtr hProcess = Kernel32.OpenProcess(ProcessRights.ALL_ACCESS, false, explorerProcessId);
            if (hProcess == IntPtr.Zero)
            {
                Debug.Assert(false);
            }

            IntPtr ipRemoteBuffer = Kernel32.VirtualAllocEx(hProcess, IntPtr.Zero, new UIntPtr(bufferSize),
                MemAllocationType.COMMIT, MemoryProtection.PAGE_READWRITE);

            if (ipRemoteBuffer == IntPtr.Zero)
            {
                Debug.Assert(false);
            }

            fixed (TrayIcon* trayIconPointer = &trayIcon)
            {
                var trayIconPointer2 = new IntPtr(trayIconPointer);

                int b = (int) User32.SendMessage(hToolbar, Toolbar.GETBUTTON, (IntPtr) i, ipRemoteBuffer);
                if (b == 0)
                {
                    Debug.Assert(false);
                }

                int dwBytesRead = 0;
                var ipBytesRead = new IntPtr(&dwBytesRead);

                bool b2 = Kernel32.ReadProcessMemory(hProcess, ipRemoteBuffer, trayIconPointer2,
                    new UIntPtr((uint) sizeof(TrayIcon)),
                    ipBytesRead);

                if (!b2)
                {
                    Debug.Assert(false);
                }
            }

            // button text
            fixed (byte* pLocalBuffer = localBuffer)
            {
                var ipLocalBuffer = new IntPtr(pLocalBuffer);

                int chars = (int) User32.SendMessage(hToolbar, Toolbar.GETBUTTONTEXTW, (IntPtr) trayIcon.idCommand, ipRemoteBuffer);
                if (chars == -1)
                {
                    Debug.Assert(false);
                }

                int dwBytesRead = 0;
                var ipBytesRead = new IntPtr(&dwBytesRead);

                bool b4 = Kernel32.ReadProcessMemory(hProcess, ipRemoteBuffer, ipLocalBuffer, new UIntPtr(bufferSize), ipBytesRead);

                if (!b4)
                {
                    Debug.Assert(false);
                }

                text = Marshal.PtrToStringUni(ipLocalBuffer, chars);

                if (text == " ")
                {
                    text = string.Empty;
                }
            }

            fixed (byte* pLocalBuffer = localBuffer)
            {
                var ipLocalBuffer = new IntPtr(pLocalBuffer);

                var ipRemoteData = new IntPtr((long) trayIcon.dwData);

                int dwBytesRead = 0;
                var ipBytesRead = new IntPtr(&dwBytesRead);

                bool b4 = Kernel32.ReadProcessMemory(
                    hProcess,
                    ipRemoteData,
                    ipLocalBuffer,
                    new UIntPtr(4),
                    ipBytesRead);

                if (!b4 || dwBytesRead != 4)
                {
                    Debug.Assert(false);
                }

                int iWindowHandle = BitConverter.ToInt32(localBuffer, 0);
                if (iWindowHandle == -1) Debug.Assert(false);

                ipWindowHandle = new IntPtr(iWindowHandle);

                User32.GetWindowThreadProcessId(ipWindowHandle, out pid);
            }

            Kernel32.VirtualFreeEx(hProcess, ipRemoteBuffer, UIntPtr.Zero, MemAllocationType.RELEASE);
            Kernel32.CloseHandle(hProcess);

            return true;
        }
    }
}