using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

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
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath) ?? Environment.CurrentDirectory);
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
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath) ?? Environment.CurrentDirectory);
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
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath) ?? Environment.CurrentDirectory);
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
        DirectoryInfo outputDirectory = new(Path.GetDirectoryName(filePath) ?? Environment.CurrentDirectory);
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

    public static void WriteToDirectory(this FileDictionary fileDictionary, string outputDirectory)
    {
        fileDictionary.WriteToDirectory(new DirectoryInfo(outputDirectory));
    }

    public static void WriteToDirectory(this FileDictionary fileDictionary, DirectoryInfo outputDirectory)
    {
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }

        foreach (var (filePath, buffer) in fileDictionary)
        {
            FileInfo fileInfo = new(Path.Join(outputDirectory.FullName, filePath));
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            using var stream = fileInfo.OpenWrite();
            stream.Write(buffer);
        }
    }

    public static async Task WriteToDirectoryAsync(this FileDictionary fileDictionary, string outputDirectory)
    {
        await fileDictionary.WriteToDirectoryAsync(new DirectoryInfo(outputDirectory));
    }

    public static async Task WriteToDirectoryAsync(this FileDictionary fileDictionary, DirectoryInfo outputDirectory)
    {
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }

        foreach (var (filePath, buffer) in fileDictionary)
        {
            FileInfo fileInfo = new(Path.Join(outputDirectory.FullName, filePath));
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            using var stream = fileInfo.OpenWrite();
            await stream.WriteAsync(buffer);
        }
    }
}
