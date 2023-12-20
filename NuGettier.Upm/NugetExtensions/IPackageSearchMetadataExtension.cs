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
}
