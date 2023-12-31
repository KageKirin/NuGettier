using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

#nullable enable

namespace NuGettier.Upm;

static class EmbeddedAssetHelper
{
    public static byte[]? GetEmbeddedResourceContent(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        if (asm != null)
        {
            if (!asm.GetManifestResourceNames().Contains(resourceName))
            {
                Console.WriteLine($"{resourceName} not found among ResourceNames");
            }

            using var stream = asm.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var source = new BinaryReader(stream);
                var fileContent = source.ReadBytes((int)stream.Length);
                return fileContent;
            }
        }

        return null;
    }

    public static string? GetEmbeddedResourceString(string resourceName)
    {
        var data = GetEmbeddedResourceContent(resourceName);
        if (data != null && data.Length > 0)
        {
            return Encoding.Default.GetString(data);
        }

        return null;
    }
}
