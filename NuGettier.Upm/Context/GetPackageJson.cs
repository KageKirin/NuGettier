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
using Xunit;

namespace NuGettier.Upm;

#nullable enable

public partial class Context
{
    public virtual async Task<PackageJson?> GetPackageJson(
        string packageIdVersion,
        bool preRelease,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        var package = await GetPackageInformation(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );

        if (package == null)
            return null;

        var packageJson = ConvertToPackageJson(package);
        Assert.NotNull(packageJson);
        if (packageJson == null)
            return null;

        return PatchPackageJson(
            packageJson: packageJson,
            prereleaseSuffix: prereleaseSuffix,
            buildmetaSuffix: buildmetaSuffix
        );
    }
}
