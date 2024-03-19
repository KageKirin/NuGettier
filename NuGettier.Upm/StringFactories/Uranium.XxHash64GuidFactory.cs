using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Standart.Hash.xxHash;
using Xunit;

namespace NuGettier.Upm.Uranium;

public class XxHash64GuidFactory : IGuidFactory, IDisposable
{
    protected readonly Microsoft.Extensions.Logging.ILogger Logger;

    readonly ulong SeedHash = 0;

    public XxHash64GuidFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, string seed)
    {
        Logger = loggerFactory.CreateLogger<XxHash64GuidFactory>();

        byte[] seedBytes = Encoding.Default.GetBytes(seed);
        SeedHash = xxHash64.ComputeHash(seedBytes, seedBytes.Length);
    }

    public virtual Guid GenerateGuid(string value)
    {
        byte[] valueBytes = Encoding.Default.GetBytes(value);
        byte[] hashUpper = BitConverter.GetBytes(xxHash64.ComputeHash(valueBytes, valueBytes.Length, SeedHash));
        byte[] hashLower = BitConverter.GetBytes(xxHash64.ComputeHash(hashUpper, hashUpper.Length, SeedHash));

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