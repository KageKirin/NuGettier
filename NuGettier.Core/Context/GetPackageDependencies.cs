using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
        string packageIdVersion,
        bool preRelease,
        CancellationToken cancellationToken
    )
    {
        var package = await GetPackageInformation(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
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
                                packageIdVersion: $"{d.Key}@{d.Value}",
                                preRelease: preRelease,
                                cancellationToken: cancellationToken
                            )
                    )
                    .Where(p => p is not null)
            ) as IEnumerable<IPackageSearchMetadata>;
    }
}
