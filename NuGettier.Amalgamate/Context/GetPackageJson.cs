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

namespace NuGettier.Amalgamate;

#nullable enable

public partial class Context
{
    public override async Task<Upm.PackageJson?> GetPackageJson(
        string packageIdVersion,
        bool preRelease,
        CancellationToken cancellationToken
    )
    {
        var packageJsonOrig = await base.GetPackageJson(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );

        if (packageJsonOrig is null)
            return null;

        PackageJson packageJson = new(packageJsonOrig);
        packageJson.Name += ".amalgamate";
        packageJson.DisplayName += " amalgamated with all dependencies";
        packageJson.Description += "\n\nThis package also contains all dependency assemblies";
        packageJson.Dependencies.Clear();
        packageJson.DevDependencies?.Clear();
        packageJson.Repository.Directory += ".amalgamate";

        return packageJson;
    }
}
