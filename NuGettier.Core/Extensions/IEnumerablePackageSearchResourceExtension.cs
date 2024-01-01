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

public static class IEnumerablePackageSearchResourceExtension
{
    public static async Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(
        this IEnumerable<PackageSearchResource> packageSearchResources,
        string searchTerm,
        SearchFilter filters,
        int skip,
        int take,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        var results = await Task.WhenAll(
            packageSearchResources.Select(
                async r =>
                    await r.SearchAsync(
                        searchTerm: searchTerm,
                        filters: filters,
                        skip: skip,
                        take: take,
                        log: logger,
                        cancellationToken: cancellationToken
                    )
            )
        );

        return results.SelectMany(s => s);
    }
}
