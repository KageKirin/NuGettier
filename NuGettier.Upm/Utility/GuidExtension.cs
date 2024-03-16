using System;

namespace NuGettier.Upm;

public static class GuidExtension
{
    public static string ToRfc4122Uuid(this Guid guid, byte version = 5)
    {
        byte[] bytes = guid.ToByteArray();
        bytes[6] = (byte)((bytes[6] & 0x0F) | (version << 4));
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        Guid uuid = new(bytes);
        return uuid.ToString("N").ToLowerInvariant();
    }
}
