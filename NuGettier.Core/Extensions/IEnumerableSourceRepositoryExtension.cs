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

public static class IEnumerableSourceRepositoryExtension
{
    public static async Task<IEnumerable<T>> GetResourceAsync<T>(
        this IEnumerable<SourceRepository> sourceRepositories,
        CancellationToken cancellationToken
    )
        where T : class, INuGetResource
    {
        return await Task.WhenAll(sourceRepositories.Select(async s => await s.GetResourceAsync<T>(cancellationToken)));
    }
}
