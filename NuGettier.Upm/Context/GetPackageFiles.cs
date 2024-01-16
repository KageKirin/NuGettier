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
using NuGettier.Upm.TarGz;
using Xunit;

namespace NuGettier.Upm;

#nullable enable

public partial class Context
{
    public virtual async Task<FileDictionary> GetPackageFiles(
        PackageArchiveReader packageReader,
        NuGetFramework nugetFramework,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        return await Task.Run(() =>
        {
            FileDictionary files = new();
            files.AddRange(packageReader.GetFrameworkFiles(nugetFramework));
            files.AddRange(packageReader.GetAdditionalFiles());
            return files;
        });
    }
}
