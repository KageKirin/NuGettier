using System.Linq;
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

    public static string? GetPreferredFramework(
        this PackageArchiveReader packageReader,
        IEnumerable<string> frameworks
    )
    {
        var assemblyFrameworks = packageReader.GetAssemblyFrameworks();

        foreach (var framework in frameworks)
        {
            if (assemblyFrameworks.Contains(framework))
                return framework;
        }

        return null;
    }

    public static TarGz.FileDictionary GetFrameworkFiles(
        this PackageArchiveReader packageReader,
        string framework
    )
    {
        return new TarGz.FileDictionary(
            packageReader
                .GetFiles("lib")
                .Where(f => Path.GetDirectoryName(f) == Path.Join("lib", framework))
                .ToDictionary(f => Path.GetFileName(f), f => packageReader.GetBytes(f))
        );
    }
}
