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
    public virtual async Task<IPackageSearchMetadata?> GetPackageInformation(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        CancellationToken cancellationToken
    )
    {
        var packages = await GetPackageVersions(
            packageName: packageName,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );

        if (latest)
        {
            return packages.FirstOrDefault();
        }

        NuGetVersion cmpVersion = new(version);
        return packages.Where(p => p.Identity.Version == cmpVersion).FirstOrDefault();
    }
}
