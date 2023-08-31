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
    public async Task<MemoryStream?> FetchPackage(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        CancellationToken cancellationToken
    )
    {
        FindPackageByIdResource resource =
            await repository.GetResourceAsync<FindPackageByIdResource>();
        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
            packageName,
            cache,
            NullLogger.Instance,
            cancellationToken
        );

        NuGetVersion? packageVersion = null;
        if (latest)
        {
            packageVersion = versions.Last();
        }
        else
        {
            packageVersion = new NuGetVersion(version);
        }

        if (
            packageVersion != null
            && await resource.DoesPackageExistAsync(
                packageName,
                packageVersion!,
                cache,
                NullLogger.Instance,
                cancellationToken
            )
        )
        {
            MemoryStream packageStream = new MemoryStream();

            await resource.CopyNupkgToStreamAsync(
                packageName,
                packageVersion!,
                packageStream,
                cache,
                NullLogger.Instance,
                cancellationToken
            );

            return packageStream;
        }
        return null;
    }
}
