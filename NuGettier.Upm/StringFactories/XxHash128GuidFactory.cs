using System;
using System.IO.Hashing;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

[GuidIdentifier("xxhash128")]
public class XxHash128GuidFactory : IGuidFactory, IDisposable
{
    protected readonly ILogger Logger;

    ulong SeedHash = 0;

    public XxHash128GuidFactory(ILogger<XxHash128GuidFactory> logger)
    {
        Logger = logger;
    }

    public virtual void InitializeWithSeed(string seed)
    {
        XxHash64 xxHash = new();
        xxHash.Append(Encoding.Default.GetBytes(seed));
        SeedHash = xxHash.GetCurrentHashAsUInt64();
    }

    public virtual Guid GenerateGuid(string value)
    {
        Guid guid = new(XxHash128.Hash(Encoding.Default.GetBytes(value), (long)SeedHash));
        Logger.LogDebug("generated GUID {0:x} for {1} with seed hash {2:x}", guid.ToString("N"), value, SeedHash);

        return guid;
    }

    public virtual void Dispose() { }
}
