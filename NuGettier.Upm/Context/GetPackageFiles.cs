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
using Xunit;
using NuGettier.Upm.TarGz;

namespace NuGettier.Upm;

#nullable enable

public partial class Context
{
    public async virtual Task<FileDictionary> GetPackageFiles(
        PackageArchiveReader packageReader,
        NuGetFramework nugetFramework,
        CancellationToken cancellationToken
    )
    {
        return await Task.Run(() =>
        {
            FileDictionary files = new();
            files.AddRange(packageReader.GetFrameworkFiles(nugetFramework));
            files.AddRange(packageReader.GetAdditionalFiles());
            return files;
        });
    }
}
