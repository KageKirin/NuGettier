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

#nullable enable

public partial class Context
{
    public virtual async Task<MemoryStream?> FetchPackage(
        string packageName,
        bool preRelease,
        bool latest,
        string? version,
        CancellationToken cancellationToken
    )
    {
        IEnumerable<FindPackageByIdResource> resources = await Repositories.GetResourceAsync<FindPackageByIdResource>(
            cancellationToken
        );
        IEnumerable<NuGetVersion> versions = (
            await resources.GetAllVersionsAsync(packageName, Cache, NullLogger.Instance, cancellationToken)
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
        if (packageVersion != null)
        {
            // return first match
            foreach (var resource in resources)
            {
                if (
                    await resource.DoesPackageExistAsync(
                        packageName,
                        packageVersion!,
                        Cache,
                        NullLogger.Instance,
                        cancellationToken
                    )
                )
                {
                    MemoryStream packageStream = new();
                    await resource.CopyNupkgToStreamAsync(
                        packageName,
                        packageVersion!,
                        packageStream,
                        Cache,
                        NullLogger.Instance,
                        cancellationToken
                    );

                    return packageStream;
                }
            }
        }

        return null;
    }
}
