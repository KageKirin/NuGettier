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

namespace NuGettier.Upm;

#nullable enable

public partial class Context
{
    public override async Task<IPackageSearchMetadata?> GetPackageInformation(
        string packageName,
        bool preRelease,
        bool latest,
        string? version,
        CancellationToken cancellationToken
    )
    {
        return await base.GetPackageInformation(packageName, preRelease, latest, version, cancellationToken);
    }
}
