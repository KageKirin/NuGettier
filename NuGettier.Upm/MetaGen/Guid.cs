using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Standart.Hash.xxHash;

namespace NuGettier.Upm.MetaGen;

struct Guid
{
    public uint128 hash;

    public static ulong SeedHash(string seed) => xxHash3.ComputeHash(seed);

    public Guid()
    {
        hash = default;
    }

    public Guid(string seed, string value)
        : this(seedHash: SeedHash(seed), value) { }

    public Guid(ulong seedHash, string value)
    {
        hash = xxHash128.ComputeHash(value, seedHash);
    }

    public override readonly string ToString()
    {
        return $"{hash.high64:x8}{hash.low64:x8}";
    }
}
