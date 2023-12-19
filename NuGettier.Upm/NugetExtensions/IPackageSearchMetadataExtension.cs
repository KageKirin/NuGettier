using System.Linq;
using System.Reflection;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Packaging.Licenses;
using NuGet.Shared;
using System.Net;

namespace NuGettier.Upm;

public static class IPackageSearchMetadataExtension
{
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
        NuGetFramework nugetFramework
    )
    {
        var packageDependencyGroup = NuGetFrameworkUtility.GetNearest<PackageDependencyGroup>(
            packageSearchMetadata.DependencySets,
            nugetFramework
        );

        if (packageDependencyGroup is null)
            return new Dictionary<string, string>();

        return packageDependencyGroup.Packages.ToDictionary(d => d.Id, d => d.VersionRange.ToLegacyShortString());
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
