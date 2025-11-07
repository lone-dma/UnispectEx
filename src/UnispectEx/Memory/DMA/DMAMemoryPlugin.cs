using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using UnispectEx.Plugins;
using VmmSharpEx;

[assembly: SupportedOSPlatform("Windows")]

namespace UnispectEx.Memory.DMA
{
    public sealed class DMAMemoryPlugin : IUnispectExPlugin
    {
        private const string MemMapPath = "mmap.txt";
        private readonly Vmm _vmm;
        private uint _pid;

        public DMAMemoryPlugin()
        {
            try
            {
                Log.Add("[DMA] Plugin Starting...");
                bool mmap = false;
                string[] args = ["-device", "FPGA", "-norefresh", "-waitinitialize"];
                if (File.Exists(MemMapPath))
                {
                    args = args.Concat(["-memmap", MemMapPath]).ToArray();
                    _vmm = new Vmm(args)
                    {
                        EnableMemoryWriting = false
                    };
                    mmap = true;
                }
                else
                {
                    _vmm = new Vmm(args)
                    {
                        EnableMemoryWriting = false
                    };
                    try
                    {
                        _vmm.GetMemoryMap(
                            applyMap: true, 
                            outputFile: MemMapPath);
                        mmap = true;
                        
                    }
                    catch // Best effort memory map
                    {
                        Log.Warn("[DMA] Failed to parse Memory Map. Will proceed without map.");
                    }
                }
                if (mmap)
                {
                    Log.Add("[DMA] Memory Map Loaded!");
                }
                Log.Add("[DMA] Plugin Loaded!");
            }
            catch (Exception ex)
            {
                Log.Exception("[DMA] ERROR Initializing FPGA", ex);
                Dispose();
                throw;
            }
        }

        public ModuleInfo GetModule(string moduleName)
        {
            try
            {
                Log.Add($"[DMA] Module Search: '{moduleName}'");
                if (!_vmm.Map_GetModuleFromName(_pid, moduleName, out var module))
                    throw new InvalidOperationException("Module not found!");
                Log.Add($"[DMA] Module Found: '{module.sText}' | Base: 0x{module.vaBase.ToString("X")} | Size: {module.cbImageSize}");
                return new ModuleInfo(moduleName, module.vaBase, (int)module.cbImageSize);
            }
            catch (Exception ex)
            {
                Log.Exception($"[DMA] ERROR retrieving module '{moduleName}'", ex);
                return null;
            }
        }

        public bool AttachToProcess(string handle)
        {
            try
            {
                Log.Add($"[DMA] Attaching to process '{handle}'");
                // Slightly differs from Unispect's default Memory Plugin.
                // Use 'ProcessName.exe' instead of 'ProcessName'.
                if (!_vmm.PidGetFromName(handle, out uint pid))
                    throw new InvalidOperationException("Process not found!");
                _pid = pid;
                return true;

            }
            catch (Exception ex)
            {
                Log.Exception($"[DMA] ERROR attaching to process '{handle}'", ex);
                return false;
            }
        }

        public byte[] Read(ulong address, int cb)
        {
            try
            {
                return _vmm.MemRead(_pid, address, (uint)cb, out _);
            }
            catch (Exception ex)
            {
                Log.Exception($"[DMA] ERROR Reading {cb} bytes at 0x{address.ToString("X")}", ex);
                return null;
            }
        }


        private bool _disposed;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, true) == false)
            {
                Log.Add("[DMA] Dispose");
                _vmm.Dispose();
            }
        }
    }
}