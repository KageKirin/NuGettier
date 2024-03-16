using System;
using System.IO.Hashing;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

public interface IGuidFactory
{
    NuGettier.Upm.MetaGen.Guid GenerateGuid(string value);
}

public class GuidFactory : IGuidFactory, IDisposable
{
    protected readonly Microsoft.Extensions.Logging.ILogger Logger;

    readonly ulong SeedHash = 0;

    public GuidFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, string seed)
    {
        Logger = loggerFactory.CreateLogger<ChangelogFactory>();

        XxHash64 xxHash = new();
        xxHash.Append(Encoding.Default.GetBytes(seed));
        SeedHash = xxHash.GetCurrentHashAsUInt64();
    }

    public virtual NuGettier.Upm.MetaGen.Guid GenerateGuid(string value)
    {
        XxHash128 xxHash = new((long)SeedHash);
        xxHash.Append(Encoding.Default.GetBytes(value));

        var hash = xxHash.GetCurrentHashAsUInt128();
        Logger.LogDebug("generated GUID {0:x} for {1} with seed hash {2:x}", hash, value, SeedHash);

        return new NuGettier.Upm.MetaGen.Guid() { hash = hash };
    }

    public virtual void Dispose() { }
}
