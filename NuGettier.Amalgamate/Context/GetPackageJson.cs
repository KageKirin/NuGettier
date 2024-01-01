using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGettier.Upm;
using Xunit;

namespace NuGettier.Amalgamate;

#nullable enable

public partial class Context
{
    public override async Task<Upm.PackageJson?> GetPackageJson(
        string packageIdVersion,
        bool preRelease,
        CancellationToken cancellationToken
    )
    {
        return await base.GetPackageJson(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );
    }
}
