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
