using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

public class Sha1GuidFactory : IGuidFactory, IDisposable
{
    protected readonly Microsoft.Extensions.Logging.ILogger Logger;

    string Seed = string.Empty;

    public Sha1GuidFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<Sha1GuidFactory>();
    }

    public virtual void InitializeWithSeed(string seed)
    {
        Seed = seed;
    }

    public virtual Guid GenerateGuid(string value)
    {
        byte[] hash = SHA1.HashData(Encoding.Default.GetBytes($"{Seed}.{value}"));
        byte[] limitedHash = new byte[16]; // Byte array for Guid must be exactly 16 bytes long.
        Array.Copy(hash, 0, limitedHash, 0, limitedHash.Length);
        Guid guid = new(limitedHash);
        Logger.LogDebug("generated GUID {0:x} for {1} with seed {2}", guid.ToString("N"), value, Seed);

        return guid;
    }

    public virtual void Dispose() { }
}
