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
    public virtual async Task<int> PublishPackedUpmPackage(
        FileInfo packageFile,
        string? token,
        string? npmrc,
        bool dryRun,
        int timeOut,
        PackageAccessLevel packageAccessLevel,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        Logger.LogTrace("publishing pre-packed UPM package for {0}", packageFile.Name);

        if (!packageFile.Exists)
        {
            Logger.TraceLocation().LogError("the package file {0} does not exist", packageFile.FullName);
            return -1;
        }

        DirectoryInfo workingDirectory = packageFile.Directory ?? new DirectoryInfo(Environment.CurrentDirectory);
        var npmrcFile = await GenerateNpmrc(
            outputDirectory: workingDirectory,
            token: token,
            npmrc: npmrc,
            cancellationToken: cancellationToken
        );

        if (!Path.Exists(npmrcFile.FullName))
        {
            Logger.TraceLocation().LogError("failed to write {0}", npmrcFile.FullName);
            return -1;
        }

        int exitCode = -2;
        try
        {
            Logger.LogTrace("creating process for `npm publish`");
            using var process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = @"npm";
            process.StartInfo.WorkingDirectory = workingDirectory.FullName;

            process.StartInfo.Arguments = string.Join(
                " ",
                "publish",
                packageFile.Name,
                $"--registry={Target.SchemelessUri()}",
                dryRun ? "--dry-run" : string.Empty,
                "--verbose",
                $"--access {packageAccessLevel.ToString().ToLowerInvariant()}"
            );

            Logger.LogDebug("starting process for `{0} {1}`", process.StartInfo.FileName, process.StartInfo.Arguments);
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
                process.Kill(entireProcessTree: true);
                Logger.LogError("process {0} has been terminated with exit code {1}", process.Id, process.ExitCode);
            }
            exitCode = process.ExitCode;

            Logger.LogTrace("reading STDOUT");
            var stdout = process.StandardOutput.ReadToEnd();
            Logger.LogTrace("read STDOUT: '{0}'", stdout);

            Logger.LogTrace("reading STDERR");
            var stderr = process.StandardError.ReadToEnd();
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

        return exitCode;
    }
}
