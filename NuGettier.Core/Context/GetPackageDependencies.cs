using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.CommandLine;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGettier.Core;

#nullable enable

public partial class Context
{
    public virtual async Task<IEnumerable<IPackageSearchMetadata>?> GetPackageDependencies(
        string packageName,
        bool preRelease,
        bool latest,
        string? version,
        CancellationToken cancellationToken
    )
    {
        var package = await GetPackageInformation(
            packageId: packageName,
            preRelease: preRelease,
            latest: latest,
            version: version,
            cancellationToken: cancellationToken
        );

        if (package is null)
            return null;

        Dictionary<string, string> dependencies = new();
        foreach (PackageDependencyGroup dependencyGroup in package.DependencySets)
        {
            foreach (PackageDependency dependency in dependencyGroup.Packages)
            {
                dependencies[dependency.Id] = dependency.VersionRange.ToLegacyShortString();
            }
        }

        return await Task.WhenAll(
                dependencies
                    .Select(
                        async (d) =>
                            await GetPackageInformation(
                                packageId: d.Key,
                                preRelease: preRelease,
                                latest: false,
                                version: d.Value,
                                cancellationToken: cancellationToken
                            )
                    )
                    .Where(p => p is not null)
            ) as IEnumerable<IPackageSearchMetadata>;
    }
}
