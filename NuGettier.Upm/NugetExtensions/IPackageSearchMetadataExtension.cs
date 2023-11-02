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
            Homepage = packageSearchMetadata.GetUpmHomepage(),
            Keywords = packageSearchMetadata.GetUpmKeywords(),
            DisplayName = packageSearchMetadata.GetUpmDisplayName(),
            Author = packageSearchMetadata.GetUpmAuthor(),
            Contributors = packageSearchMetadata.GetUpmContributors(),
            Repository = packageSearchMetadata.GetUpmRepository(),
            PublishingConfiguration = packageSearchMetadata.GetUpmPublishingConfiguration(),
            Dependencies = packageSearchMetadata.GetUpmDependencies(Context.DefaultFrameworks),
        };
    }

    public static string GetUpmPackageName(this IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Identity.Id;
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

    public static string? GetUpmHomepage(this IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.ProjectUrl?.ToString();
    }

    public static IEnumerable<string> GetUpmKeywords(this IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Tags.Split(',', ';', ' ').Where(t => !string.IsNullOrEmpty(t));
    }

    public static Person GetUpmAuthor(this IPackageSearchMetadata packageSearchMetadata)
    {
        var firstAuthor = packageSearchMetadata.Authors.Split(',', ';', ' ').First();
        if (string.IsNullOrEmpty(firstAuthor))
        {
            firstAuthor = packageSearchMetadata.Owners.Split(',', ';', ' ').First();
        }

        if (string.IsNullOrEmpty(firstAuthor))
        {
            firstAuthor = @"unknown author, early 21st century";
        }

        return new Person() { Name = firstAuthor, };
    }

    public static IEnumerable<Person> GetUpmContributors(this IPackageSearchMetadata packageSearchMetadata)
    {
        var otherAuthors = packageSearchMetadata.Authors.Split(',', ';', ' ');
        if (otherAuthors.Length <= 1)
            return new List<Person>();

        return otherAuthors[1..].Select(author => new Person() { Name = author, });
    }

    public static Repository GetUpmRepository(this IPackageSearchMetadata packageSearchMetadata)
    {
        return new Repository()
        {
            RepoType = @"git",
            Url = packageSearchMetadata.ProjectUrl.ToString(),
            Directory = packageSearchMetadata.Identity.Id.ToLowerInvariant(),
        };
    }

    public static PublishingConfiguration GetUpmPublishingConfiguration(
        this IPackageSearchMetadata packageSearchMetadata
    )
    {
        return new PublishingConfiguration() { Registry = string.Empty, };
    }

    public static IDictionary<string, string> GetUpmDependencies(
        this IPackageSearchMetadata packageSearchMetadata,
        IEnumerable<string> frameworks
    )
    {
        var framework = packageSearchMetadata.GetUpmPreferredFramework(frameworks);
        return new StringStringDictionary(
            packageSearchMetadata.DependencySets
                .Where(dependencyGroup => dependencyGroup.TargetFramework.GetShortFolderName() == framework)
                .SelectMany(dependencyGroup => dependencyGroup.Packages)
                .ToDictionary(d => d.Id, d => d.VersionRange.ToLegacyShortString())
        );
    }

    public static string GetUpmPreferredFramework(
        this IPackageSearchMetadata packageSearchMetadata,
        IEnumerable<string> frameworks
    )
    {
        var ourFrameworks = packageSearchMetadata.DependencySets
            .Select(dependencyGroup => dependencyGroup.TargetFramework.GetShortFolderName())
            .ToHashSet();
        return ourFrameworks.Intersect(frameworks).OrderDescending().FirstOrDefault(frameworks.First());
    }

    public static string GetUpmPreferredUnityVersion(
        this IPackageSearchMetadata packageSearchMetadata,
        IDictionary<string, string> unityFrameworks
    )
    {
        var framework = packageSearchMetadata.GetUpmPreferredFramework(unityFrameworks.Keys);
        return unityFrameworks[framework];
    }
}
