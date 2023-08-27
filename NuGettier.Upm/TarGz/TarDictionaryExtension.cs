using System;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;

namespace NuGettier.Upm.TarGz;

public static class TarDictionaryExtension
{
    public static TarDictionary ToTarDictionary(this TarInputStream tis)
    {
        var tarDictionary = new TarDictionary();
        while (true)
        {
            var entry = tis.GetNextEntry();
            if (entry == null)
                break;

            var buffer = new byte[entry.Size];
            var read = tis.Read(buffer, 0, buffer.Length);

            tarDictionary.Add(entry.Name, buffer);
        }

        return tarDictionary;
    }

    public static TarOutputStream FromTarDictionary(
        this TarOutputStream tos,
        TarDictionary tarDictionary
    )
    {
        foreach (var (k, v) in tarDictionary)
        {
            var entry = TarEntry.CreateTarEntry(k);
            entry.Size = v.Length;
            tos.PutNextEntry(entry);
            tos.Write(v, 0, v.Length);
            tos.CloseEntry();
        }
        return tos;
    }
}
