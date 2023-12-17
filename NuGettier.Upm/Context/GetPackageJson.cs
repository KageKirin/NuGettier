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

namespace NuGettier.Upm;

#nullable enable

public partial class Context
{
    public virtual async Task<PackageJson?> GetPackageJson(
        string packageId,
        bool preRelease,
        bool latest,
        string? version,
        CancellationToken cancellationToken
    )
    {
        var package = await GetPackageInformation(
            packageId: packageId,
            preRelease: preRelease,
            latest: latest,
            version: version,
            cancellationToken: cancellationToken
        );

        if (package == null)
            return null;

        var packageJson = package.ToPackageJson(NugetFramework, MinUnityVersion);
        Assert.NotNull(packageJson);
        if (packageJson == null)
            return null;

        return PatchPackageJson(packageJson);
    }
}
