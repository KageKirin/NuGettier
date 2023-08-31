using System;
using System.Text;
using NuGettier.Upm.TarGz;

namespace NuGettier.Upm;

public class Package : IDisposable
{
    public string Name;
    public TarGz.TarDictionary Files { get; protected set; } = new TarGz.TarDictionary();

    public Templates.Readme Readme { get; protected set; }
    public PackageJson PackageJson { get; protected set; }

    public Package(string name)
    {
        Name = name;
        PackageJson = new();
        Readme = new();
    }

    public void Dispose()
    {
        Files.Dispose();
    }

    public void CompleteFiles()
    {
        Add(Readme);
        Add(PackageJson);
    }

    public void Add(string filename, string textFileContents, bool addMeta)
    {
        Add(filename, Encoding.ASCII.GetBytes(textFileContents), addMeta);
    }

    public void Add(string filename, byte[] binFileContents, bool addMeta)
    {
        if (!filename.StartsWith(@"package/"))
        {
            filename = $"package/{filename}";
        }

        Files.Add(filename, binFileContents);

        if (Path.GetExtension(filename) != @".meta" && addMeta)
        {
            Files.Add(
                $"{filename}.meta",
                Encoding.ASCII.GetBytes((Upm.MetaGen.GenerateMeta(PackageJson.Name, filename)))
            );
        }
    }

    public void Add(Templates.Readme readme)
    {
        Add("README.md", readme.ToString(), addMeta: true);
    }

    public void Add(PackageJson packageJson)
    {
        if (string.IsNullOrEmpty(Name))
        {
            Name = $"{packageJson.Name}-{packageJson.Version}.tgz";
        }

        Add("package.json", packageJson.ToJson(), addMeta: true);
    }

    public void WriteToTarGz(string filename)
    {
        Files.WriteToTarGz(filename);
    }

    public Stream WriteToTarGz(Stream outStream)
    {
        Files.WriteToTarGz(outStream);
        return outStream;
    }

    public async Task WriteToTarGzAsync(string filename)
    {
        await Files.WriteToTarGzAsync(filename);
    }

    public async Task<Stream> WriteToTarGzAsync(Stream outStream)
    {
        await Files.WriteToTarGzAsync(outStream);
        return outStream;
    }
}
