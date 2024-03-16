using System;
using System.IO.Hashing;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm.MetaGen;

public struct Guid
{
    public UInt128 hash;

    public static UInt64 SeedHash(string seed)
    {
        XxHash64 xxHash = new();
        xxHash.Append(Encoding.Default.GetBytes(seed));
        return xxHash.GetCurrentHashAsUInt64();
    }

    public Guid()
    {
        hash = default;
    }

    public Guid(string seed, string value)
        : this(seedHash: SeedHash(seed), value) { }

    public Guid(ulong seedHash, string value)
    {
        XxHash128 xxHash = new((long)seedHash);
        xxHash.Append(Encoding.Default.GetBytes(value));
        hash = xxHash.GetCurrentHashAsUInt128();
    }

    public override readonly string ToString()
    {
        return $"{hash:x}";
    }
}
