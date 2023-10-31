using System.Linq;
using System.Reflection;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Packaging.Licenses;
using NuGet.Shared;
using System.Net;

namespace NuGettier.Upm;

public static class IPackageSearchMetadataExtension
{
    public static PackageJson ToPackageJson(this IPackageSearchMetadata packageSearchMetadata)
    {
        return new PackageJson()
        {
            Name = packageSearchMetadata.GetUpmPackageName(),
            Version = packageSearchMetadata.GetUpmVersion(),
            License = packageSearchMetadata.GetUpmLicense() ?? string.Empty,
            Description = packageSearchMetadata.GetUpmDescription(),
            Keywords = packageSearchMetadata.GetUpmKeywords(),
            DisplayName = packageSearchMetadata.GetUpmDisplayName(),
            Author = packageSearchMetadata.GetUpmAuthor(),
            Repository = packageSearchMetadata.GetUpmRepository(),
            PublishingConfiguration = packageSearchMetadata.GetUpmPublishingConfiguration(),
            Dependencies = packageSearchMetadata.GetUpmDependencies(),
        };
    }

    public static string GetUpmPackageName(this IPackageSearchMetadata packageSearchMetadata)
    {
        return NuspecReaderExtension.GetUpmPackageName(
            packageSearchMetadata.Authors,
            packageSearchMetadata.Identity.Id
        );
        ;
    }

    public static string GetUpmVersion(this IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Identity.Version.ToString();
    }

    public static string? GetUpmLicense(this IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.LicenseMetadata == null
            ? string.Empty
            : packageSearchMetadata.LicenseMetadata.LicenseExpression != null
                ? packageSearchMetadata.LicenseMetadata.LicenseExpression.ToString()
                : !string.IsNullOrEmpty(packageSearchMetadata.LicenseMetadata.License)
                    ? packageSearchMetadata.LicenseMetadata.License
                    : packageSearchMetadata.LicenseMetadata.LicenseUrl.ToString();
    }

    public static string GetUpmDescription(this IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Description;
    }

    public static string GetUpmDisplayName(this IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Title;
    }

    public static IEnumerable<string> GetUpmKeywords(this IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Tags.Split(',', ';', ' ').Where(t => !string.IsNullOrEmpty(t));
    }

    public static Person GetUpmAuthor(this IPackageSearchMetadata packageSearchMetadata)
    {
        return new Person()
        {
            Name = packageSearchMetadata.Authors,
            Url = packageSearchMetadata.ProjectUrl?.ToString(),
        };
    }

    public static Repository GetUpmRepository(this IPackageSearchMetadata packageSearchMetadata)
    {
        return new Repository()
        {
            RepoType = string.Empty,
            Url = string.Empty,
            Directory = string.Empty,
        };
    }

    public static PublishingConfiguration GetUpmPublishingConfiguration(
        this IPackageSearchMetadata packageSearchMetadata
    )
    {
        return new PublishingConfiguration() { Registry = string.Empty, };
    }

    public static IDictionary<string, string> GetUpmDependencies(this IPackageSearchMetadata packageSearchMetadata)
    {
        var framework = packageSearchMetadata.DependencySets
            .Select(dependencyGroup => dependencyGroup.TargetFramework.GetShortFolderName())
            .Where(framework => Context.DefaultFrameworks.Contains(framework))
            .FirstOrDefault(Context.DefaultFrameworks.First());

        return new StringStringDictionary(
            packageSearchMetadata.DependencySets
                .Where(dependencyGroup => dependencyGroup.TargetFramework.GetShortFolderName() == framework)
                .SelectMany(dependencyGroup => dependencyGroup.Packages)
                .ToDictionary(d => d.Id, d => d.VersionRange.ToLegacyShortString())
        );
    }
}
