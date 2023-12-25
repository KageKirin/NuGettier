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
using NuGettier.Core;

namespace NuGettier.Upm;

#nullable enable

public partial class Context
{
    public override async Task<IPackageSearchMetadata?> GetPackageInformation(
        string packageIdVersion,
        bool preRelease,
        CancellationToken cancellationToken
    )
    {
        packageIdVersion.SplitPackageIdVersion(out var packageId, out var version, out var latest);
        var packageSearchMetadata = await base.GetPackageInformation(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );
        if (packageSearchMetadata != null)
        {
            CachedMetadata[packageId.ToLowerInvariant()] = packageSearchMetadata;

            var packageDependencyGroup = NuGetFrameworkUtility.GetNearest<PackageDependencyGroup>(
                packageSearchMetadata.DependencySets,
                NugetFramework
            );

            if (packageDependencyGroup is null)
                return null;

            var dependencyPackageSearchMetadata = await Task.WhenAll(
                packageDependencyGroup.Packages.Select(
                    async dependency =>
                        await base.GetPackageInformation(
                            packageIdVersion: $"{dependency.Id}@{dependency.VersionRange.ToLegacyShortString()}",
                            preRelease: true,
                            cancellationToken: cancellationToken
                        )
                )
            );

            if (dependencyPackageSearchMetadata != null)
            {
                foreach (var metadata in dependencyPackageSearchMetadata)
                {
                    if (metadata != null)
                        CachedMetadata[metadata.Identity.Id.ToLowerInvariant()] = metadata;
                }
            }
        }

        return packageSearchMetadata;
    }
}
