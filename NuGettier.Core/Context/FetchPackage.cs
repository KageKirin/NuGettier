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
        Logger.LogTrace("fetching {0} (prerelease: {1})", packageIdVersion, preRelease);
        using var nugetLogger = NuGetLogger.Create(LoggerFactory);

        packageIdVersion.SplitPackageIdVersion(out var packageId, out var version, out var latest);
        IEnumerable<FindPackageByIdResource> resources = await Repositories.GetResourceAsync<FindPackageByIdResource>(
            cancellationToken
        );
        Logger.LogTrace("trying to get '{0}' version {1} (latest: {2})", packageId, version, latest);

        IEnumerable<NuGetVersion> versions = (
            await resources.GetAllVersionsAsync(packageId, Cache, nugetLogger, cancellationToken)
        ).Distinct();

        NuGetVersion? packageVersion = default;
        if (latest)
        {
            Logger.LogTrace("fetching latest version for {0}", packageId);
            packageVersion = versions.Last();
            Logger.LogTrace("latest version is {0}", packageVersion);
        }
        else if (version != null)
        {
            Logger.LogTrace("fetching version {1} for {0}", packageId, version);
            packageVersion = new NuGetVersion(version);
            Logger.LogTrace("required version is {0}", packageVersion);
        }
        Logger.LogDebug("fetching {0}@{1}", packageId, packageVersion);

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
                Logger.LogDebug("checking if package {0}@{1} exists on {2}", packageId, packageVersion, resource);
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

        Logger.TraceLocation().LogTrace("exit {0} returning null", this.__METHOD__());
        return null;
    }
}
