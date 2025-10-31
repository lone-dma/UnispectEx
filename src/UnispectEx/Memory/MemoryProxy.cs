using System;
using System.Runtime.CompilerServices;
using UnispectEx.Plugins;

namespace UnispectEx.Memory
{
    /// <summary>
    /// Proxy class for memory reading using a loaded IUnispectExPlugin.
    /// Implements additional helper methods.
    /// </summary>
    public sealed class MemoryProxy : IDisposable
    {
        /// <summary>
        /// Singleton instance of the loaded MemoryProxy.
        /// </summary>
        public static MemoryProxy Instance { get; private set; }

        private readonly IUnispectExPlugin _plugin;

        private MemoryProxy() => throw new NotImplementedException();
        private MemoryProxy(IUnispectExPlugin plugin)
        {
            ArgumentNullException.ThrowIfNull(plugin, nameof(plugin));
            _plugin = plugin;
        }

        /// <summary>
        /// Creates a memory proxy with the given type at runtime.
        /// </summary>
        /// <param name="pluginType">Type to instantiate. Must have a paramaterless constructor.</param>
        /// <returns></returns>
        /// <exception cref="TypeLoadException"></exception>
        public static MemoryProxy Create(Type pluginType)
        {
            if (Activator.CreateInstance(pluginType) is IUnispectExPlugin plugin)
            {
                return Instance = new MemoryProxy(plugin);
            }
            throw new TypeLoadException($"Failed to instantiate '{pluginType}' Plugin Type.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttachToProcess(string handle) => _plugin.AttachToProcess(handle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ModuleInfo GetModule(string moduleName) => _plugin.GetModule(moduleName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Read(ulong address, int cb) => _plugin.Read(address, cb);

        public T Read<T>(ulong address)
            where T : unmanaged
        {
            int cb = Unsafe.SizeOf<T>();
            var result = _plugin.Read(address, cb);
            if (result is null)
                return default;
            return Unsafe.As<byte, T>(ref result[0]);
        }

        public void Dispose()
        {
            Instance = null;
            _plugin.Dispose();
        }
    }
}
