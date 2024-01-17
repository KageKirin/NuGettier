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
            int exitCode = -2;
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

            if (token != null)
            {
                Logger.LogTrace("writing .npmrc to {0}", tempDir.FullName);
                using var npmrcWriter = new StreamWriter(File.OpenWrite(Path.Join(tempDir.FullName, ".npmrc")));

                // format is "//${schemeless_registry}/:_authToken=${token}"
                npmrcWriter.WriteLine($"//{Target.SchemelessUri()}:_authToken={token}");
            }
            else if (npmrc != null)
            {
                Logger.LogTrace("copying .npmrc to {0}", tempDir.FullName);
                File.Copy(npmrc, Path.Join(tempDir.FullName, ".npmrc"));
            }

            try
            {
                Logger.LogTrace("creating process for `npm publish`");
                using var process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.FileName = @"npm";
                process.StartInfo.WorkingDirectory = tempDir.FullName;

                process.StartInfo.Arguments = string.Join(
                    " ",
                    "publish",
                    packageFile.Name,
                    $"--registry={Target.SchemelessUri()}",
                    dryRun ? "--dry-run" : string.Empty,
                    "--verbose",
                    $"--access {packageAccessLevel.ToString().ToLowerInvariant()}"
                );

                Logger.LogDebug(
                    "starting process for `{0} {1}`",
                    process.StartInfo.FileName,
                    process.StartInfo.Arguments
                );
                process.Start();

                Logger.LogDebug(
                    "started process for {0} (waiting {1} seconds or until termination to proceed)",
                    process.Id,
                    timeOut
                );
                if (process.WaitForExit(timeOut * 1000))
                {
                    Logger.LogDebug("process {0} has terminated with exit code {1}", process.Id, process.ExitCode);
                }
                else
                {
                    Logger.LogError("process {0} has timed out and will now be terminated", process.Id);
                    process.Kill();
                    Logger.LogError("process {0} has been terminated with exit code {1}", process.Id, process.ExitCode);
                }
                exitCode = process.ExitCode;

                Logger.LogTrace("reading STDOUT");
                var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                Logger.LogTrace("read STDOUT: '{0}'", stdout);

                Logger.LogTrace("reading STDERR");
                var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
                Logger.LogTrace("read STDERR: '{0}'", stderr);

                if (!string.IsNullOrEmpty(stdout))
                    Logger.LogInformation($"NPM: {stdout}");

                if (!string.IsNullOrEmpty(stderr))
                    Logger.LogError($"NPM: {stderr}");
            }
            catch (Exception e)
            {
                Logger.LogError($"NPM: {e.Message}");
            }

            Logger.LogTrace("deleting temp working directory: {0}", tempDir.FullName);
            tempDir.Delete(recursive: true);
            return exitCode;
        }
    }
}
