using System;

namespace NuGettier.Upm.MetaGen;

public struct Guid
{
    public UInt128 hash;

    public Guid()
    {
        hash = default;
    }

    public Guid(UInt128 hash)
    {
        this.hash = hash;
    }

    public override readonly string ToString()
    {
        return $"{hash:x}";
    }
}
