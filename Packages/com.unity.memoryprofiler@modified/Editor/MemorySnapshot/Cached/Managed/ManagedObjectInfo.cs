namespace Unity.MemoryProfilerExtension.Editor
{
    public struct ManagedObjectInfo
    {
        public ulong PtrObject;
        public ulong PtrTypeInfo;
        public long NativeObjectIndex;
        public long ManagedObjectIndex;
        public int ITypeDescription;
        public long Size;
        public int RefCount;
        public BytesAndOffset data;

        public bool IsKnownType => ITypeDescription >= 0;

        public static ManagedObjectInfo Invalid => new ManagedObjectInfo() { NativeObjectIndex = -1, ManagedObjectIndex = -1 };

        public bool IsValid()
        {
            return PtrObject != 0 && PtrTypeInfo != 0 && data.Bytes.IsCreated;
        }

        public static bool operator ==(ManagedObjectInfo lhs, ManagedObjectInfo rhs)
        {
            return lhs.PtrObject == rhs.PtrObject
                && lhs.PtrTypeInfo == rhs.PtrTypeInfo
                && lhs.NativeObjectIndex == rhs.NativeObjectIndex
                && lhs.ManagedObjectIndex == rhs.ManagedObjectIndex
                && lhs.ITypeDescription == rhs.ITypeDescription
                && lhs.Size == rhs.Size
                && lhs.RefCount == rhs.RefCount;
        }

        public static bool operator !=(ManagedObjectInfo lhs, ManagedObjectInfo rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            if (obj is ManagedObjectInfo otherObj)
            {
                return this == otherObj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (PtrObject, PtrTypeInfo, NativeObjectIndex, ManagedObjectIndex, ITypeDescription, Size, RefCount).GetHashCode();
        }
    }
}
