using System;
using VmmSharpEx;

namespace unispectDMAPlugin
{
    internal static class Extensions
    {
        /// <summary>
        /// Custom Memory Read Method.
        /// Does not resize partial reads.
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="va"></param>
        /// <param name="cb"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static unsafe byte[] MemReadCustom(this Vmm vmm, uint pid, ulong va, int cb)
        {
            var buffer = new byte[cb];
            fixed (byte* pb = buffer)
            {
                if (!vmm.MemRead(pid, va, pb, (uint)cb, out _))
                    throw new Exception("Memory Read Failed!");
            }
            return buffer;
        }
    }
}
