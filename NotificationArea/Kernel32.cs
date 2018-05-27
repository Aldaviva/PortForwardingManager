using System;
using System.Runtime.InteropServices;

namespace NotificationArea
{
    internal static class ProcessRights
    {
        private const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private const uint SYNCHRONIZE = 0x00100000;

        internal const uint ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF;
    }

    internal static class MemoryProtection
    {
        public const uint PAGE_READWRITE = 0x04;
    }

    internal static class MemAllocationType
    {
        public const uint COMMIT = 0x1000;
        public const uint RELEASE = 0x8000;
    }

    internal static class Kernel32
    {
        private const string KERNEL32 = "kernel32.dll";

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType,
            uint flProtect);

        [DllImport(KERNEL32)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, UIntPtr nSize,
            IntPtr lpNumberOfBytesRead);

        [DllImport(KERNEL32)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        [DllImport(KERNEL32, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);
    }
}