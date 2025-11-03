using System;

namespace UnispectEx.Plugins
{
    /// <summary>
    /// Defines the interface for UnispectEx plugins.
    /// </summary>
    /// <remarks>
    /// Must be a public type and contain a parameterless constructor.
    /// </remarks>
    public interface IUnispectExPlugin : IDisposable
    {
        /// <summary>
        /// Retrieves a process module by its name.
        /// </summary>
        /// <param name="moduleName">Name of the module to lookup.</param>
        /// <returns><see cref="ModuleInfo"/> instance containing Name, Base Address, and Size.</returns>
        ModuleInfo GetModule(string moduleName);

        /// <summary>
        /// Attach to the target process.
        /// </summary>
        /// <param name="handle">The process handle or name.</param>
        /// <returns>True if the attachment was successful, otherwise false.</returns>
        bool AttachToProcess(string handle);
        
        /// <summary>
        /// Reads memory from the target process.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <param name="cb">The number of bytes to read.</param>
        /// <returns>A byte array containing the read memory, or NULL if failed.</returns>
        byte[] Read(ulong address, int cb);
    }
}
