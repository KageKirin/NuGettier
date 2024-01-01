using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
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

public static class IEnumerablePackageMetadataResourceExtension
{
    public static async Task<IEnumerable<IPackageSearchMetadata>> GetMetadataAsync(
        this IEnumerable<PackageMetadataResource> packageMetadataResources,
        string packageId,
        bool includePrerelease,
        bool includeUnlisted,
        SourceCacheContext sourceCacheContext,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        var results = await Task.WhenAll(
            packageMetadataResources.Select(
                r =>
                    r.GetMetadataAsync(
                        packageId: packageId,
                        includePrerelease: includePrerelease,
                        includeUnlisted: includeUnlisted,
                        sourceCacheContext: sourceCacheContext,
                        log: logger,
                        token: cancellationToken
                    )
            )
        );

        return results.SelectMany(s => s);
    }
}
