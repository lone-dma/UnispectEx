namespace UnispectEx.Plugins
{
    public sealed class ModuleInfo
    {
        public string Name { get; }
        public ulong BaseAddress { get; }
        public int Size { get; } 

        public ModuleInfo(string name, ulong baseAddress, int size)
        {
            Name = name;
            BaseAddress = baseAddress;
            Size = size;
        }

    }
}