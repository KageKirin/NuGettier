using System;

namespace NuGettier.Upm;

public static class GuidExtension
{
    /// <summary>
    /// transforms the input Guid into a RFC-4122 UUID
    /// </summary>
    /// <see href="https://datatracker.ietf.org/doc/html/rfc4122#section-4.1.6" />
    /// <param name="guid">a Guid</param>
    /// <param name="version">indicates the RFC-4122 'version' parameter, which is '5' for SHA1 UUIDs</param>
    /// <returns>the same Guid with a few bits changed</returns>
    /// <remarks>
    /// discovered this in Xoofx' UnityNuGet source code
    /// https://github.com/xoofx/UnityNuGet
    /// </remarks>
    public static Guid ToRfc4122(this Guid guid, byte version = 5)
    {
        byte[] bytes = guid.ToByteArray();
        bytes[6] = (byte)((bytes[6] & 0x0F) | (version << 4));
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        return new Guid(bytes);
    }

    /// <summary>
    /// returns a Unity-compatible Guid string
    /// </summary>
    /// <param name="guid">a Guid</param>
    /// <returns>lowercase hexadecimal character string without separators</returns>
    public static string ToUnityString(this Guid guid)
    {
        return guid.ToString("N").ToLowerInvariant();
    }
}
