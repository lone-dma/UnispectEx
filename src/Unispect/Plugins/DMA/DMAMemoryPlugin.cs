using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using VmmSharpEx;

[assembly: SupportedOSPlatform("Windows")]

namespace Unispect.Plugins.DMA
{
    [UnispectPlugin]
    public sealed class DMAMemoryPlugin : MemoryProxy
    {
        private const string MemMapPath = "mmap.txt";
        private readonly Vmm _vmm;
        private uint _pid;

        public DMAMemoryPlugin()
        {
            try
            {
                Log.Add("[DMA] Plugin Starting...");
                if (File.Exists(MemMapPath))
                {
                    Log.Add("[DMA] Memory Map Found!");
                    _vmm = new Vmm("-device", "FPGA", "-memmap", MemMapPath, "-waitinitialize");
                }
                else
                    _vmm = new Vmm("-device", "FPGA", "-waitinitialize");
                Log.Add("[DMA] Plugin Loaded!");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("[DMA] ERROR Initializing FPGA", ex);
            }
        }

        public override ModuleProxy GetModule(string moduleName)
        {
            try
            {
                Log.Add($"[DMA] Module Search: '{moduleName}'");
                if (!_vmm.Map_GetModuleFromName(_pid, moduleName, out var module))
                    throw new InvalidOperationException("Module not found!");
                Log.Add($"[DMA] Module Found: '{module.sText}' | Base: 0x{module.vaBase.ToString("X")} | Size: {module.cbImageSize}");
                return new ModuleProxy(moduleName, module.vaBase, (int)module.cbImageSize);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"[DMA] ERROR retrieving module '{moduleName}'", ex);
            }
        }

        public override bool AttachToProcess(string handle)
        {
            try
            {
                Log.Add($"[DMA] Attaching to process '{handle}'");
                // Slightly differs from Unispect's default Memory Plugin.
                // Use 'ProcessName.exe' instead of 'ProcessName'.
                if (!_vmm.PidGetFromName(handle, out uint pid))
                    throw new Exception("Process not found!");
                _pid = pid;
                return true;

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"[DMA] ERROR attaching to process '{handle}'", ex);
            }
        }

        public override byte[] Read(ulong address, int length)
        {
            try
            {
                return _vmm.MemReadCustom(_pid, address, length);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"[DMA] ERROR Reading {length} bytes at 0x{address.ToString("X")}", ex);
            }
        }


        private bool _disposed;
        public override void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, true) == false)
            {
                Log.Add("[DMA] Dispose");
                _vmm.Dispose();
            }
        }
    }
}