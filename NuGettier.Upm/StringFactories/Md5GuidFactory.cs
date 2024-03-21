using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

public class Md5GuidFactory : IGuidFactory, IDisposable
{
    protected readonly ILogger Logger;

    string Seed = string.Empty;

    public Md5GuidFactory(ILogger<Md5GuidFactory> logger)
    {
        Logger = logger;
    }

    public virtual void InitializeWithSeed(string seed)
    {
        Seed = seed;
    }

    public virtual Guid GenerateGuid(string value)
    {
        byte[] hash = MD5.HashData(Encoding.Default.GetBytes($"{Seed}.{value}"));
        byte[] limitedHash = new byte[16]; // Byte array for Guid must be exactly 16 bytes long.
        Array.Copy(hash, 0, limitedHash, 0, limitedHash.Length);
        Guid guid = new(limitedHash);
        Logger.LogDebug("generated GUID {0:x} for {1} with seed {2}", guid.ToString("N"), value, Seed);

        return guid;
    }

    public virtual void Dispose() { }
}
