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
using Xunit;
using NuGettier.Upm;
using NuGettier.Upm.TarGz;

namespace NuGettier.Amalgamate;

#nullable enable

public partial class Context
{
    public async override Task<FileDictionary> GetPackageFiles(
        PackageArchiveReader packageReader,
        NuGetFramework nugetFramework,
        CancellationToken cancellationToken
    )
    {
        FileDictionary files = await base.GetPackageFiles(packageReader, nugetFramework, cancellationToken);

        var packageRule = GetPackageRule(packageReader.NuspecReader.GetIdentity().Id);
        Assert.NotNull(packageRule);
        if (packageRule.IsRecursive)
        {
            var packageDependencyGroup = NuGetFrameworkUtility.GetNearest<PackageDependencyGroup>(
                packageReader.NuspecReader.GetDependencyGroups(),
                NugetFramework
            );

            if (packageDependencyGroup is not null)
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
                        continue;

                    using PackageArchiveReader dependencyPackageReader = new(dependencyPackageStream);
                    files.AddRange(await GetPackageFiles(dependencyPackageReader, nugetFramework, cancellationToken));
                }
            }
        }

        return files;
    }
}
