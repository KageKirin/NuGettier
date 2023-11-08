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

namespace NuGettier.Upm;

#nullable enable

public partial class Context
{
    public override async Task<IPackageSearchMetadata?> GetPackageInformation(
        string packageName,
        bool preRelease,
        bool latest,
        string? version,
        CancellationToken cancellationToken
    )
    {
        var packageSearchMetadata = await base.GetPackageInformation(
            packageName,
            preRelease,
            latest,
            version,
            cancellationToken
        );
        if (packageSearchMetadata != null)
        {
            CachedMetadata[packageName.ToLowerInvariant()] = packageSearchMetadata;

            var dependencies = packageSearchMetadata.DependencySets
                .Where(
                    dependencyGroup =>
                        dependencyGroup.TargetFramework.GetShortFolderName()
                        == packageSearchMetadata.GetUpmPreferredFramework(Frameworks)
                )
                .SelectMany(dependencyGroup => dependencyGroup.Packages)
                .Distinct();

            var dependencyPackageSearchMetadata = await Task.WhenAll(
                dependencies.Select(
                    async dependency =>
                        await base.GetPackageInformation(
                            packageName: dependency.Id,
                            preRelease: true,
                            latest: false,
                            version: dependency.VersionRange.ToLegacyShortString(),
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
