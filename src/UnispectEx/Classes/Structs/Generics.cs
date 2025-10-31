namespace UnispectEx
{
    // For more detail see: https://github.com/Unity-Technologies/mono/blob/unity-2018.4-mbe/mono/metadata/class-internals.h 
    public struct MonoGenericClass
    {
        public ulong ContainerClass; // MonoClass 0x0
        public MonoGenericContext Context;
    }

    public struct MonoGenericContext
    {
        public ulong ClassInstance; // MonoGenericInstance
        public ulong MethodInstance;
    }

    public struct MonoGenericInstance
    {
        public const int MaxParams = 10;
        public readonly int Id;
        public readonly int BitField;

        // If there are instances where the params go over 10, probably want to investigate it manually...
        // Because Something<t1,t2,t3,t4,t5,t6,t7,t8,t9,t10> seems pretty nuts.
        private unsafe fixed ulong _monoTypes[MaxParams];

        public unsafe readonly ulong GetType(uint index) => _monoTypes[index];
    }
}