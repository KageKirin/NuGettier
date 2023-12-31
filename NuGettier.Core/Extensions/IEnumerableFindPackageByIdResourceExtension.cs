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

public static class IEnumerableFindPackageByIdResourceExtension
{
    public static async Task<IEnumerable<bool>> DoesPackageExistAsync(
        this IEnumerable<FindPackageByIdResource> findPackageByIdResources,
        string id,
        NuGetVersion version,
        SourceCacheContext cacheContext,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        return await Task.WhenAll(
            findPackageByIdResources.Select(
                r =>
                    r.DoesPackageExistAsync(
                        id: id,
                        version: version,
                        cacheContext: cacheContext,
                        logger: logger,
                        cancellationToken: cancellationToken
                    )
            )
        );
    }

    public static async Task<IEnumerable<NuGetVersion>> GetAllVersionsAsync(
        this IEnumerable<FindPackageByIdResource> findPackageByIdResources,
        string id,
        SourceCacheContext cacheContext,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        var results = await Task.WhenAll(
            findPackageByIdResources.Select(
                async r =>
                    await r.GetAllVersionsAsync(
                        id: id,
                        cacheContext: cacheContext,
                        logger: logger,
                        cancellationToken: cancellationToken
                    )
            )
        );
        return results.SelectMany(s => s);
    }
}
