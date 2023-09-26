using System.Linq;
using System.Reflection;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;

namespace NuGettier.Upm;

public static class NuspecReaderExtension
{
    // put here to keep package name formatting in 1 single location
    public static string GetUpmPackageName(this IPackageSearchMetadata searchMetadata)
    {
        return GetUpmPackageName(searchMetadata.Authors, searchMetadata.Identity.Id);
    }

    public static string GetUpmPackageName(this NuspecReader nuspecReader)
    {
        return GetUpmPackageName(nuspecReader.GetAuthors(), nuspecReader.GetId());
    }

    public static string GetUpmPackageName(string author, string id)
    {
        // TODO: use config string + Handlebars template
        return $"com.{author}.{id}".ToLowerInvariant().Replace(@" ", @"");
    }

    public static string GetUpmVersion(
        this NuspecReader nuspecReader,
        string? prereleaseSuffix = null,
        string? buildmetaSuffix = null
    )
    {
        var version = nuspecReader.GetVersion().ToString();
        if (prereleaseSuffix != null)
            version += $"-{prereleaseSuffix}";
        if (buildmetaSuffix != null)
            version += $"+{buildmetaSuffix}";

        return version;
    }

    public static string GetUpmName(this NuspecReader nuspecReader)
    {
        return string.IsNullOrWhiteSpace(nuspecReader.GetTitle()) ? nuspecReader.GetId() : nuspecReader.GetTitle();
    }

    public static string GetUpmDisplayName(this NuspecReader nuspecReader, string framework, AssemblyName assemblyName)
    {
        return nuspecReader.GetUpmName()
            + $" ({framework} DLL)"
            + $" [repacked by {assemblyName.Name} v{assemblyName.Version.ToString()}]";
    }

    public static List<string> GetUpmKeywords(this NuspecReader nuspecReader)
    {
        List<string> keywords = new();
        if (!string.IsNullOrWhiteSpace(nuspecReader.GetTags()))
        {
            keywords.AddRange(nuspecReader.GetTags().Split(@" "));
        }
        return keywords;
    }

    public static Author GetUpmAuthor(this NuspecReader nuspecReader)
    {
        return new Author() { Name = nuspecReader.GetAuthors(), Url = nuspecReader.GetProjectUrl(), };
    }

    public static PackageJson.StringStringDictionary GetUpmDependencies(
        this NuspecReader nuspecReader,
        string framework
    )
    {
        return new PackageJson.StringStringDictionary(
            nuspecReader
                .GetDependencyGroups()
                .Where(d => d.TargetFramework.GetShortFolderName() == framework)
                .SelectMany(d => d.Packages)
                .ToDictionary(
                    p => p.Id, // TODO: use GetUpmPackageName
                    p => p.VersionRange.ToLegacyShortString()
                )
        );
    }

    public static Repository GetUpmRepository(this NuspecReader nuspecReader)
    {
        return new Repository() { Url = nuspecReader.GetProjectUrl(), };
    }

    public static PublishingConfiguration GetUpmPublishingConfiguration(
        this NuspecReader nuspecReader,
        Uri targetRegistry
    )
    {
        return new PublishingConfiguration() { Registry = targetRegistry.ToString(), };
    }

    public static string GenerateUpmReadme(
        this NuspecReader nuspecReader,
        AssemblyName assemblyName,
        string? prereleaseSuffix = null,
        string? buildmetaSuffix = null
    )
    {
        return ReadmeStringFactory.GenerateReadme(
            name: $"{nuspecReader.GetUpmName()} ({nuspecReader.GetUpmPackageName()})",
            version: nuspecReader.GetUpmVersion(prereleaseSuffix, buildmetaSuffix),
            description: nuspecReader.GetDescription(),
            applicationName: assemblyName.Name,
            applicationVersion: assemblyName.Version.ToString()
        );
    }

    public static string GenerateUpmLicense(
        this NuspecReader nuspecReader,
        string? prereleaseSuffix = null,
        string? buildmetaSuffix = null
    )
    {
        return LicenseStringFactory.GenerateLicense(
            name: nuspecReader.GetUpmName(),
            version: nuspecReader.GetUpmVersion(prereleaseSuffix, buildmetaSuffix),
            copyright: nuspecReader.GetCopyright(),
            copyrightHolder: string.IsNullOrEmpty(nuspecReader.GetOwners())
                ? nuspecReader.GetAuthors()
                : nuspecReader.GetOwners(),
            license: nuspecReader.GetLicenseMetadata()?.License,
            licenseUrl: nuspecReader.GetLicenseUrl()
        );
    }

    public static string GenerateUpmChangelog(this NuspecReader nuspecReader)
    {
        return ChangelogStringFactory.GenerateChangelog(
            name: nuspecReader.GetUpmName(),
            version: nuspecReader.GetUpmVersion(),
            releaseNotes: nuspecReader.GetReleaseNotes()
        );
    }

    public static PackageJson GenerateUpmPackageJson(
        this NuspecReader nuspecReader,
        string framework,
        Uri targetRegistry,
        AssemblyName assemblyName
    )
    {
        PackageJson packageJson =
            new()
            {
                Name = nuspecReader.GetUpmPackageName(),
                Version = nuspecReader.GetUpmVersion(),
                License = nuspecReader.GetLicenseMetadata()?.License,
                Description = nuspecReader.GetDescription(),
                Keywords = nuspecReader.GetUpmKeywords(),
                DisplayName = nuspecReader.GetUpmDisplayName(framework, assemblyName),
                Repository = nuspecReader.GetUpmRepository(),
                PublishingConfiguration = nuspecReader.GetUpmPublishingConfiguration(targetRegistry),
                Dependencies = nuspecReader.GetUpmDependencies(framework),
            };

        return packageJson;
    }
}
