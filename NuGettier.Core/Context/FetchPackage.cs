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

#nullable enable

public partial class Context
{
    public virtual async Task<MemoryStream?> FetchPackage(
        string packageIdVersion,
        bool preRelease,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        using var nugetLogger = NuGetLogger.Create(LoggerFactory);

        packageIdVersion.SplitPackageIdVersion(out var packageId, out var version, out var latest);
        IEnumerable<FindPackageByIdResource> resources = await Repositories.GetResourceAsync<FindPackageByIdResource>(
            cancellationToken
        );
        IEnumerable<NuGetVersion> versions = (
            await resources.GetAllVersionsAsync(packageId, Cache, nugetLogger, cancellationToken)
        ).Distinct();

        NuGetVersion? packageVersion = default;
        if (latest)
        {
            packageVersion = versions.Last();
        }
        else if (version != null)
        {
            packageVersion = new NuGetVersion(version);
        }

        // no assert here
        // `null` is a valid case when latest==true and no version could not be retrieved (b/c package doesn't exist, e.g.)
        if (packageVersion == null)
        {
            Logger.TraceLocation().LogError("package version is null, meaning 'latest' version could not be retrieved");
        }
        else
        {
            // return first match
            foreach (var resource in resources)
            {
                if (
                    await resource.DoesPackageExistAsync(
                        packageId,
                        packageVersion!,
                        Cache,
                        nugetLogger,
                        cancellationToken
                    )
                )
                {
                    MemoryStream packageStream = new();
                    await resource.CopyNupkgToStreamAsync(
                        packageId,
                        packageVersion!,
                        packageStream,
                        Cache,
                        nugetLogger,
                        cancellationToken
                    );

                    return packageStream;
                }
            }
        }

        return null;
    }
}
