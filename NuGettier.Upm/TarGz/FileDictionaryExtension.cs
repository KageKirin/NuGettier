using System;
using System.Text;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;

namespace NuGettier.Upm.TarGz;

public static class FileDictionaryExtension
{
    public static TarDictionary ToTarDictionary(this GZipInputStream gzStream)
    {
        using (var tarStream = new TarInputStream(gzStream, Encoding.Default))
        {
            return tarStream.ToTarDictionary();
        }
    }

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

    public static async Task<TarOutputStream> FromTarDictionaryAsync(
        this TarOutputStream tos,
        TarDictionary tarDictionary
    )
    {
        return await new Task<TarOutputStream>(() => tos.FromTarDictionary(tarDictionary));
    }

    public static void WriteToTar(this TarDictionary tarDictionary, string filePath)
    {
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath));
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }

        FileInfo outputFile = new(filePath);
        tarDictionary.WriteToTar(outputFile.OpenWrite());
    }

    public static Stream WriteToTar(this TarDictionary tarDictionary, Stream outStream)
    {
        using (TarOutputStream tarStream = new(outStream, Encoding.Default))
        {
            tarStream.FromTarDictionary(tarDictionary);
        }
        return outStream;
    }

    public static async Task WriteToTarAsync(this TarDictionary tarDictionary, string filePath)
    {
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath));
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }

        FileInfo outputFile = new(filePath);
        await tarDictionary.WriteToTarAsync(outputFile.OpenWrite());
    }

    public static async Task<Stream> WriteToTarAsync(
        this TarDictionary tarDictionary,
        Stream outStream
    )
    {
        using (TarOutputStream tarStream = new(outStream, Encoding.Default))
        {
            await tarStream.FromTarDictionaryAsync(tarDictionary);
        }
        return outStream;
    }

    public static void WriteToTarGz(this TarDictionary tarDictionary, string filePath)
    {
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath));
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }

        FileInfo outputFile = new(filePath);
        tarDictionary.WriteToTarGz(outputFile.OpenWrite());
    }

    public static Stream WriteToTarGz(this TarDictionary tarDictionary, Stream outStream)
    {
        using (GZipOutputStream gzStream = new(outStream))
        {
            tarDictionary.WriteToTar(gzStream);
        }
        return outStream;
    }

    public static async Task WriteToTarGzAsync(this TarDictionary tarDictionary, string filePath)
    {
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath));
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }

        FileInfo outputFile = new(filePath);
        await tarDictionary.WriteToTarGzAsync(outputFile.OpenWrite());
    }

    public static async Task<Stream> WriteToTarGzAsync(
        this TarDictionary tarDictionary,
        Stream outStream
    )
    {
        using (GZipOutputStream gzStream = new(outStream))
        {
            return await tarDictionary.WriteToTarAsync(gzStream);
        }
    }
}
