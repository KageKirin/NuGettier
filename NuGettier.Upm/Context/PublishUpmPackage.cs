using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Reflection;
using System.CommandLine;
using System.CommandLine.IO;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
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
        PackageAccessLevel packageAccessLevel,
        CancellationToken cancellationToken
    )
    {
        var tuple = await PackUpmPackage(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            prereleaseSuffix: prereleaseSuffix,
            buildmetaSuffix: buildmetaSuffix,
            cancellationToken: cancellationToken
        );

        if (tuple == null)
            return -1;

        var (packageIdentifier, package) = tuple!;
        using (package)
        {
            int exitCode = -2;
            var tempDir = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
            var packageFile = $"{packageIdentifier}.tgz";
            await package.WriteToTarGzAsync(Path.Join(tempDir, packageFile));

            if (token != null)
            {
                using var npmrcWriter = new StreamWriter(File.OpenWrite(Path.Join(tempDir, ".npmrc")));

                // format is "//${schemeless_registry}/:_authToken=${token}"
                npmrcWriter.WriteLine($"//{Target.SchemelessUri()}:_authToken={token}");
            }
            else if (npmrc != null)
            {
                File.Copy(npmrc, Path.Join(tempDir, ".npmrc"));
            }

            try
            {
                using var process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.FileName = @"npm";
                process.StartInfo.WorkingDirectory = tempDir;

                process.StartInfo.Arguments = string.Join(
                    " ",
                    "publish",
                    packageFile,
                    $"--registry={Target.SchemelessUri()}",
                    dryRun ? "--dry-run" : string.Empty,
                    "--verbose",
                    $"--access {packageAccessLevel.ToString().ToLowerInvariant()}"
                );

                process.Start();
                await process.WaitForExitAsync(cancellationToken);
                exitCode = process.ExitCode;
                var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

                if (!string.IsNullOrEmpty(stdout))
                    Console.WriteLine($"NPM: {stdout}");

                if (!string.IsNullOrEmpty(stderr))
                    Console.Error.WriteLine($"NPM: {stderr}");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"NPM: {e.Message}");
            }

            System.IO.Directory.Delete(tempDir, recursive: true);
            return exitCode;
        }
    }
}
