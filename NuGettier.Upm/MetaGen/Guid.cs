using System;
using System.Text;
using Standart.Hash.xxHash;

namespace NuGettier.Upm;

public static partial class MetaGen
{
    struct Guid
    {
        public ulong upper;
        public ulong lower;

        public Guid()
        {
            upper = 0;
            lower = 0;
        }

        public Guid(string seed, string value)
        {
            var seedHash = xxHash64.ComputeHash(Encoding.UTF8.GetBytes(seed));
            upper = xxHash64.ComputeHash(Encoding.UTF8.GetBytes(value), seedHash);
            lower = xxHash64.ComputeHash(Encoding.UTF8.GetBytes(value), upper);
        }

        public override readonly string ToString()
        {
            return $"{upper:x8}{lower:x8}";
        }
    }
}
