using System.Linq;
using System.Text;
using NuGet.Configuration;
using NuGet.Frameworks;
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
            .Select(f => Path.GetDirectoryName(f) ?? string.Empty)
            .Where(f => f != null)
            .Select(f => f!)
            .Select(f => f.Replace("lib/", string.Empty))
            .Distinct()
            .OrderDescending()
            .ToList();
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

    public static TarGz.FileDictionary GetFrameworkFiles(
        this PackageArchiveReader packageReader,
        NuGetFramework framework
    )
    {
        var frameworkSpecificGroup = NuGetFrameworkUtility.GetNearest<FrameworkSpecificGroup>(
            packageReader.GetLibItems(),
            framework
        );

        if (frameworkSpecificGroup is null)
            return new TarGz.FileDictionary();

        return new TarGz.FileDictionary(
            frameworkSpecificGroup.Items.ToDictionary(
                f => $"{packageReader.NuspecReader.GetId()}/{f}", //< explicitly forward slashes '/' for paths
                f => packageReader.GetBytes(f)
            )
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
                .ToDictionary(
                    f => $"{packageReader.NuspecReader.GetId()}/{f}", //< explicitly forward slashes '/' for paths
                    f => packageReader.GetBytes(f)
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
