using System.Linq;
using System.Text;
using NuGet.Packaging;

namespace NuGettier.Upm;

public static class PackageArchiveReaderExtension
{
    public static byte[] GetBytes(this PackageArchiveReader packageReader, string file)
    {
        using var stream = packageReader.GetStream(file);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.GetBuffer();
    }

    public static IEnumerable<string> GetAssemblyFrameworks(this PackageArchiveReader packageReader)
    {
        return packageReader
            .GetFiles("lib")
            .Select(f => Path.GetDirectoryName(f))
            .Select(f => f?.Replace("lib/", string.Empty))
            .Distinct()
            .OrderDescending();
    }

    public static string SelectPreferredFramework(
        this PackageArchiveReader packageReader,
        IEnumerable<string> frameworks
    )
    {
        var assemblyFrameworks = packageReader.GetAssemblyFrameworks();
        Console.WriteLine($"assemblyFrameworks: {string.Join(", ", assemblyFrameworks)}");

        foreach (var framework in frameworks)
        {
            if (assemblyFrameworks.Contains(framework))
                return framework;
        }

        return string.Empty;
    }

    public static TarGz.FileDictionary GetFrameworkFiles(this PackageArchiveReader packageReader, string framework)
    {
        return new TarGz.FileDictionary(
            packageReader
                .GetFiles("lib")
                .Where(f => f.Contains(framework))
                .ToDictionary(f => f, f => packageReader.GetBytes(f))
        );
    }

    public static TarGz.FileDictionary GetAdditionalFiles(
        this PackageArchiveReader packageReader,
        NuspecReader nuspecReader,
        bool renameOriginalMarkdownFiles
    )
    {
        List<string> additionalFiles =
            new()
            {
                nuspecReader.GetIcon(),
                nuspecReader.GetReadme(),
                nuspecReader.GetReleaseNotes(),
                "LICENSE.TXT",
                "LICENSE.md",
                "LICENSE",
            };

        return new TarGz.FileDictionary(
            packageReader
                .GetFiles()
                .Where(f => additionalFiles.Contains(f))
                .Select(
                    f =>
                        new KeyValuePair<string, byte[]>(
                            renameOriginalMarkdownFiles && Path.GetExtension(f) == ".md"
                                ? $"{Path.GetFileName(f)}.orig.{Path.GetExtension(f)}"
                                : f,
                            packageReader.GetBytes(f)
                        )
                )
        );
    }

    public static byte[]? GetReadmeFile(this PackageArchiveReader packageReader, NuspecReader nuspecReader)
    {
        if (string.IsNullOrEmpty(nuspecReader.GetReadme()))
            return null;

        return packageReader.GetBytes(nuspecReader.GetReadme());
    }

    public static string GetReadme(this PackageArchiveReader packageReader, NuspecReader nuspecReader)
    {
        byte[]? data = packageReader.GetReadmeFile(nuspecReader);
        if (data == null || data.Length == 0)
            return string.Empty;

        return Encoding.Default.GetString(data);
    }

    public static byte[]? GetLicenseFile(this PackageArchiveReader packageReader, NuspecReader nuspecReader)
    {
        var packageFiles = packageReader.GetFiles();
        foreach (var f in packageFiles)
        {
            if (Path.GetFileName(f).ToLowerInvariant() == @"license")
                packageReader.GetBytes(f);
        }

        return null;
    }

    public static string GetLicense(this PackageArchiveReader packageReader, NuspecReader nuspecReader)
    {
        byte[]? data = packageReader.GetLicenseFile(nuspecReader);
        if (data == null || data.Length == 0)
            return string.Empty;

        return Encoding.Default.GetString(data);
    }
}
