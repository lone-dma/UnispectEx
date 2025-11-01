using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnispectEx.Plugins;

namespace UnispectEx.Memory
{
    /// <summary>
    /// Basic memory plugin using Windows API (RPM) for Memory Reading.
    /// </summary>
    public sealed class BasicMemory : IUnispectExPlugin
    {
        private Process _process;
        private IntPtr _handle;

        public ModuleInfo GetModule(string moduleName)
        {
            if (_process is null) 
                throw new InvalidOperationException("Not currently attached to a process.");

            foreach (ProcessModule pm in _process.Modules)
            {
                if (pm.ModuleName?.EndsWith(moduleName, StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    return new ModuleInfo(pm.ModuleName, (ulong)pm.BaseAddress.ToInt64(), pm.ModuleMemorySize);
                }
            }

            return null;
        }

        public bool AttachToProcess(string handle)
        {
            var procList = Process.GetProcessesByName(handle);

            if (procList.Length == 0)
                return false;

            _process = procList[0];
            _handle = OpenProcess(ProcessVmAll, false, _process.Id);

            return _handle != IntPtr.Zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Read(ulong address, int cb) => RPM(address, cb);

        private byte[] RPM(ulong address, int cb)
        {
            var buffer = new byte[cb];
            if (ReadProcessMemory(_handle, address, buffer, cb, out int cbRead) && cbRead > 0)
            {
                return buffer;
            }

            return null;
        }

        public void Dispose()
        {
            _process = null;
            if (_handle != IntPtr.Zero)
            {
                CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }
        }

        #region Win32 Imports

        private const int ProcessVmAll = ProcessVmOperation | ProcessVmRead | ProcessVmWrite;
        private const int ProcessVmOperation = 0x0008;
        private const int ProcessVmRead = 0x0010;
        private const int ProcessVmWrite = 0x0020;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, ulong lpAddress, byte[] buffer, int size, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hHandle);

        #endregion
    }
}