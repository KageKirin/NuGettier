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

namespace NuGettier.Core;

public partial class Context
{
    public virtual async Task<IEnumerable<IPackageSearchMetadata>> SearchPackages(
        string searchTerm,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        IEnumerable<PackageSearchResource> resources = await Repositories.GetResourceAsync<PackageSearchResource>(
            cancellationToken
        );
        SearchFilter searchFilter = new(includePrerelease: true);

        using var nugetLogger = NuGetLogger.Create(LoggerFactory);
        return await resources.SearchAsync(
            searchTerm,
            searchFilter,
            skip: 0,
            take: 100,
            nugetLogger,
            cancellationToken
        );
    }
}
