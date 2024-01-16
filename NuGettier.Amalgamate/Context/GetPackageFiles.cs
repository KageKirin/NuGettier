using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGettier.Upm;
using NuGettier.Upm.TarGz;
using Xunit;

namespace NuGettier.Amalgamate;

#nullable enable

public partial class Context
{
    public override async Task<FileDictionary> GetPackageFiles(
        PackageArchiveReader packageReader,
        NuGetFramework nugetFramework,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        FileDictionary files = await base.GetPackageFiles(packageReader, nugetFramework, cancellationToken);

        var packageRule = GetPackageRule(packageReader.NuspecReader.GetIdentity().Id);
        Assert.NotNull(packageRule);
        if (packageRule.IsRecursive)
        {
            Logger.LogDebug(
                "recursing dependencies for {0}@{1}",
                packageReader.NuspecReader.GetIdentity().Id,
                packageReader.NuspecReader.GetIdentity().Version
            );
            var packageDependencyGroup = NuGetFrameworkUtility.GetNearest<PackageDependencyGroup>(
                packageReader.NuspecReader.GetDependencyGroups(),
                NugetFramework
            );

            if (packageDependencyGroup is null)
            {
                Logger.TraceLocation().LogError("returned PackageDependencyGroup is null");
            }
            else
            {
                foreach (var dependency in packageDependencyGroup.Packages)
                {
                    var dependencyPackageRule = GetPackageRule(dependency.Id);
                    Assert.NotNull(dependencyPackageRule);
                    if (dependencyPackageRule.IsIgnored)
                        continue;

                    if (dependencyPackageRule.IsExcluded)
                        continue;

                    using var dependencyPackageStream = await FetchPackage(
                        packageIdVersion: $"{dependency.Id}@{dependency.VersionRange.ToLegacyShortString()}",
                        preRelease: true,
                        cancellationToken: cancellationToken
                    );
                    if (dependencyPackageStream == null)
                    {
                        Logger.TraceLocation().LogError("fetched PackageStream is null");
                        continue;
                    }

                    using PackageArchiveReader dependencyPackageReader = new(dependencyPackageStream);
                    var packageFiles = await GetPackageFiles(
                        packageReader: dependencyPackageReader,
                        nugetFramework: nugetFramework,
                        cancellationToken: cancellationToken
                    );
                    files.AddRange(packageFiles.Where(f => !files.ContainsKey(f.Key)));
                }
            }
        }

        return files;
    }
}
