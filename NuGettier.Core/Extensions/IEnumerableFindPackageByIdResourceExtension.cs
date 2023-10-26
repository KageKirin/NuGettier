using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Microsoft.VisualBasic;
using System.Net.Http.Headers;

namespace NuGettier.Core;

#nullable enable

public static class IEnumerableFindPackageByIdResourceExtension
{
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
