using System;
using System.Text;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;

namespace NuGettier.Upm.TarGz;

public static class FileDictionaryExtension
{
    const string RootPath = @"package";

    public static FileDictionary ToFileDictionary(this GZipInputStream gzStream)
    {
        using (var tarStream = new TarInputStream(gzStream, Encoding.Default))
        {
            return tarStream.ToFileDictionary();
        }
    }

    public static FileDictionary ToFileDictionary(this TarInputStream tis)
    {
        var fileDictionary = new FileDictionary();
        while (true)
        {
            var entry = tis.GetNextEntry();
            if (entry == null)
                break;

            var buffer = new byte[entry.Size];
            var read = tis.Read(buffer, 0, buffer.Length);

            var filePath = entry.Name;
            if (filePath.StartsWith(RootPath))
                filePath.Replace($"{RootPath}/", "");
            fileDictionary.Add(filePath, buffer);
        }

        return fileDictionary;
    }

    public static TarOutputStream FromFileDictionary(this TarOutputStream tos, FileDictionary fileDictionary)
    {
        foreach (var (k, v) in fileDictionary)
        {
            var filePath = Path.GetPathRoot(k) == RootPath ? k : Path.Join(RootPath, k);
            var entry = TarEntry.CreateTarEntry(filePath);
            entry.Size = v.Length;
            tos.PutNextEntry(entry);
            tos.Write(v, 0, v.Length);
            tos.CloseEntry();
        }
        return tos;
    }

    public static async Task<TarOutputStream> FromFileDictionaryAsync(
        this TarOutputStream tos,
        FileDictionary fileDictionary
    )
    {
        return await Task.Run(() => tos.FromFileDictionary(fileDictionary));
    }

    public static void WriteToTar(this FileDictionary fileDictionary, string filePath)
    {
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath));
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }

        FileInfo outputFile = new(filePath);
        fileDictionary.WriteToTar(outputFile.OpenWrite());
    }

    public static Stream WriteToTar(this FileDictionary fileDictionary, Stream outStream)
    {
        using (TarOutputStream tarStream = new(outStream, Encoding.Default))
        {
            tarStream.FromFileDictionary(fileDictionary);
        }
        return outStream;
    }

    public static async Task WriteToTarAsync(this FileDictionary fileDictionary, string filePath)
    {
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath));
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }

        FileInfo outputFile = new(filePath);
        await fileDictionary.WriteToTarAsync(outputFile.OpenWrite());
    }

    public static async Task<Stream> WriteToTarAsync(this FileDictionary fileDictionary, Stream outStream)
    {
        using (TarOutputStream tarStream = new(outStream, Encoding.Default))
        {
            await tarStream.FromFileDictionaryAsync(fileDictionary);
        }
        return outStream;
    }

    public static void WriteToTarGz(this FileDictionary fileDictionary, string filePath)
    {
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath));
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }

        FileInfo outputFile = new(filePath);
        fileDictionary.WriteToTarGz(outputFile.OpenWrite());
    }

    public static Stream WriteToTarGz(this FileDictionary fileDictionary, Stream outStream)
    {
        using (GZipOutputStream gzStream = new(outStream))
        {
            fileDictionary.WriteToTar(gzStream);
        }
        return outStream;
    }

    public static async Task WriteToTarGzAsync(this FileDictionary fileDictionary, string filePath)
    {
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath));
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }

        FileInfo outputFile = new(filePath);
        using var stream = await fileDictionary.WriteToTarGzAsync(outputFile.OpenWrite());
    }

    public static async Task<Stream> WriteToTarGzAsync(this FileDictionary fileDictionary, Stream outStream)
    {
        using (GZipOutputStream gzStream = new(outStream))
        {
            return await fileDictionary.WriteToTarAsync(gzStream);
        }
    }
}
