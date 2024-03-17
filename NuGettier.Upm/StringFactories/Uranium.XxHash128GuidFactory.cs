using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Standart.Hash.xxHash;

namespace NuGettier.Upm.Uranium;

public class XxHash128GuidFactory : IGuidFactory, IDisposable
{
    protected readonly Microsoft.Extensions.Logging.ILogger Logger;

    readonly ulong SeedHash = 0;

    public XxHash128GuidFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, string seed)
    {
        Logger = loggerFactory.CreateLogger<XxHash128GuidFactory>();

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
