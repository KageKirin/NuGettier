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

public partial class Context
{
    public async Task<IEnumerable<IPackageSearchMetadata>> SearchPackages(
        string searchTerm,
        CancellationToken cancellationToken
    )
    {
        SearchFilter searchFilter = new(includePrerelease: true);

        List<IPackageSearchMetadata> results = new();
        foreach (var repository in Repositories)
        {
            PackageSearchResource resource = await repository.GetResourceAsync<PackageSearchResource>(
                cancellationToken
            );

            results.AddRange(
                await resource.SearchAsync(
                    searchTerm,
                    searchFilter,
                    skip: 0,
                    take: 100,
                    NullLogger.Instance,
                    cancellationToken
                )
            );
        }

        return results;
    }
}
