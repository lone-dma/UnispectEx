﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using MahApps.Metro.Converters;

namespace Unispect
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public struct FieldDefinition
    {
        public ulong Type;
        public ulong NamePtr;
        public ulong Parent;
        public int Offset;
        private int pad0; // align(8)

        public string Name
        {
            get
            {
                if (CacheStore.FieldNameCache.ContainsKey(NamePtr + Type))
                    return CacheStore.FieldNameCache[NamePtr + Type];

                var name = GetName();
                CacheStore.FieldNameCache.AddOrUpdate(NamePtr, name, (arg1, s) => s);
                return name;
            }
        }

        private string GetName()
        {
            if (NamePtr < 0x10000000 || Offset > 0x2000)
                return "<ErrorReadingField_OutOfRange>";

            byte[] buffer = Memory.Read(NamePtr, 1024);
            if (buffer == null)
                return "<ErrorReadingField>";

            return buffer.ReadName();
        }

        public override string ToString()
        {
            return Name;
        }

        public bool HasValue(out string valueType)
        {
            var monoType = Memory.Read<MonoType>(Type);

            if (monoType.HasValue)
            {
                if (monoType.IsConstant) valueType = "Constant";
                else if (monoType.IsStatic) valueType = "Static";
                else valueType = "Unknown";

                return true;
            }

            valueType = "";
            return false;
        }

        public TypeEnum TypeCode
        {
            get
            {
                var monoType = Memory.Read<MonoType>(Type);
                var typeCode = monoType.TypeCode;
                return typeCode;
            }
        } 

        public string GetFieldTypeString()
        {
            var monoType = Memory.Read<MonoType>(Type);

            var typeCode = monoType.TypeCode;
            switch (typeCode)
            {
                case TypeEnum.Class:
                case TypeEnum.SzArray:
                case TypeEnum.GenericInst:
                case TypeEnum.ValueType:
                    var typeDef = Memory.Read<TypeDefinition>(Memory.Read<ulong>(monoType.Data));
                    var name = typeDef.GetFullName();

                    // Potential bug, not all genericinst are valid? Needs further investigation.
                    // Temporary fix by using a stack overflow protection counter
                    if (typeCode == TypeEnum.GenericInst)
                    {
                        // If the field type is a generic instance, grab the generic parameters
                        var stackProtectionCounter = 0;
                        name = GetGenericParams(name, monoType, ref stackProtectionCounter);
                    }

                    if (typeCode == TypeEnum.SzArray)
                    {
                        name += "[]";
                    }

                    return name;

                default:
                    return Enum.GetName(typeof(TypeEnum), typeCode);
            }
        }

        private string GetGenericParams(string name, MonoType monoType, ref int stackProtectionCounter)
        {
            if (stackProtectionCounter++ > 30) 
                return "StackOverflow";

            var genericIndexOf = name.IndexOf('`');
            if (genericIndexOf >= 0)
            {
                // Remove the generic disclaimer
                name = name.Replace(name.Substring(genericIndexOf), "");
            }

            var genericParams = "";

            var monoGenericClass = Memory.Read<MonoGenericClass>(monoType.Data);
            var monoGenericInst =
                Memory.Read<MonoGenericInstance>(monoGenericClass.Context.ClassInstance);

            var paramCount = monoGenericInst.BitField & 0x003fffff; // (1 << 22) - 1;

            for (uint i = 0; i < paramCount && i < MonoGenericInstance.MaxParams; i++)
            {
                var subType = MemoryProxy.Instance.Read<MonoType>(monoGenericInst.MonoTypes[i]);
                var subTypeCode = subType.TypeCode;

                switch (subTypeCode)
                {
                    case TypeEnum.Class:
                    case TypeEnum.SzArray:
                    case TypeEnum.GenericInst:
                    case TypeEnum.ValueType:
                        var subTypeDef = Memory.Read<TypeDefinition>(Memory.Read<ulong>(subType.Data));
                        var subName = subTypeDef.Name;
                        if (subTypeCode == TypeEnum.GenericInst)
                            genericParams += GetGenericParams(subName, subType, ref stackProtectionCounter); // Recursive to determine nested types
                        else
                            genericParams += $"{subName}, ";
                        break;
                    default:
                        genericParams += $"{Enum.GetName(typeof(TypeEnum), subTypeCode)}, ";
                        break;
                }

            }

            genericParams = genericParams.TrimEnd(',', ' ');
            name += $"<{genericParams}>";

            return name;
        }

        public TypeDefinition? GetFieldType()
        {
            var monoType = Memory.Read<MonoType>(Type);

            var typeCode = monoType.TypeCode;
            switch (typeCode)
            {
                case TypeEnum.Class:
                case TypeEnum.SzArray:
                case TypeEnum.GenericInst: // todo check generic types
                case TypeEnum.ValueType:
                    var typeDef = Memory.Read<TypeDefinition>(Memory.Read<ulong>(monoType.Data));
                    return typeDef;
            }

            return null;
        }

        public static MemoryProxy Memory => MemoryProxy.Instance;
    }
}