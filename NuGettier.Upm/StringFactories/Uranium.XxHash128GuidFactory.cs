using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Standart.Hash.xxHash;

namespace NuGettier.Upm.Uranium;

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
        byte[] seedBytes = Encoding.Default.GetBytes(seed);
        SeedHash = xxHash64.ComputeHash(seedBytes, seedBytes.Length);
    }

    public virtual Guid GenerateGuid(string value)
    {
        byte[] valueBytes = Encoding.Default.GetBytes(value);
        uint128 h128 = xxHash128.ComputeHash(valueBytes, valueBytes.Length, SeedHash);
        Guid guid = h128.ToGuid();
        Logger.LogDebug("generated GUID {0:x} for {1} with seed hash {2:x}", guid.ToString("N"), value, SeedHash);

        return guid;
    }

    public virtual void Dispose() { }
}
