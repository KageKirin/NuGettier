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
            var data = Encoding.UTF8.GetBytes(value);
            upper = xxHash3.ComputeHash(data, data.Length, seedHash);
            lower = xxHash3.ComputeHash(data, data.Length, upper);
        }

        public override readonly string ToString()
        {
            return $"{upper:x8}{lower:x8}";
        }
    }
}
