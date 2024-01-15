using System;
using System.Text;
using Standart.Hash.xxHash;

namespace NuGettier.Upm;

public static partial class MetaFactory
{
    struct Guid
    {
        public uint128 hash;

        public Guid()
        {
            hash = default;
        }

        public Guid(string seed, string value)
        {
            var seedHash = xxHash3.ComputeHash(seed);
            hash = xxHash128.ComputeHash(value, seedHash);
        }

        public override readonly string ToString()
        {
            return $"{hash.high64:x8}{hash.low64:x8}";
        }
    }
}
