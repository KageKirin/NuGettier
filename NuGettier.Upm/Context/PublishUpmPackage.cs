using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
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

namespace NuGettier.Upm;

public partial class Context
{
    public virtual async Task<int> PublishUpmPackage(
        string packageIdVersion,
        bool preRelease,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        string? token,
        string? npmrc,
        bool dryRun,
        int timeOut,
        PackageAccessLevel packageAccessLevel,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        Logger.LogTrace("creating UPM package for {0}", packageIdVersion);
        var tuple = await PackUpmPackage(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            prereleaseSuffix: prereleaseSuffix,
            buildmetaSuffix: buildmetaSuffix,
            cancellationToken: cancellationToken
        );

        if (tuple == null)
        {
            Logger.TraceLocation().LogError("failed to pack UPM package for {0}", packageIdVersion);
            return -1;
        }

        var (packageIdentifier, package) = tuple!;
        using (package)
        {
            DirectoryInfo tempDir = new(Path.Join(Path.GetTempPath(), Path.GetRandomFileName()));
            Logger.LogDebug("temp working directory: {0}", tempDir.FullName);

            FileInfo packageFile = new(Path.Join(tempDir.FullName, $"{packageIdentifier}.tgz"));
            Logger.LogTrace("writing package file: {0}", packageFile.FullName);
            await package.WriteToTarGzAsync(packageFile.FullName);
            if (!packageFile.Exists)
            {
                Logger.TraceLocation().LogError("failed to write {0}", packageFile.FullName);
                return -1;
            }

            int exitCode = await PublishPackedUpmPackage(
                packageFile: packageFile,
                token: token,
                npmrc: npmrc,
                dryRun: dryRun,
                timeOut: timeOut,
                packageAccessLevel: packageAccessLevel,
                cancellationToken: cancellationToken
            );

            if (exitCode != 0)
            {
                Logger
                    .TraceLocation()
                    .LogError("failed to publish UPM package for {0} (error code: {1})", packageIdVersion, exitCode);
            }

            Logger.LogTrace("deleting temp working directory: {0}", tempDir.FullName);
            tempDir.Delete(recursive: true);
            return exitCode;
        }
    }
}
