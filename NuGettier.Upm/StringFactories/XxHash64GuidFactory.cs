using System;
using System.IO.Hashing;
using System.Text;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NuGettier.Upm;

public class XxHash64GuidFactory : IGuidFactory, IDisposable
{
    protected readonly ILogger Logger;

    ulong SeedHash = 0;

    public XxHash64GuidFactory(ILogger<XxHash64GuidFactory> logger)
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
        byte[] hashUpper = XxHash64.Hash(Encoding.Default.GetBytes(value), (long)SeedHash);
        byte[] hashLower = XxHash64.Hash(hashUpper, (long)SeedHash);

        Assert.Equal(8, hashUpper.Length);
        Assert.Equal(8, hashLower.Length);

        byte[] compositeHash = new byte[16]; // Byte array for Guid must be exactly 16 bytes long.
        Array.Copy(hashUpper, 0, compositeHash, 0, hashUpper.Length);
        Array.Copy(hashLower, 0, compositeHash, hashUpper.Length, hashLower.Length);
        Guid guid = new(compositeHash);
        Logger.LogDebug("generated GUID {0:x} for {1} with seed hash {2:x}", guid.ToString("N"), value, SeedHash);

        return guid;
    }

    public virtual void Dispose() { }
}
