using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Reflection;
using System.Text;
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
using NuGettier.Upm;
using NuGettier.Upm.TarGz;
using Xunit;

namespace NuGettier;

public partial class NuGettierService
{
    private Command AmalgamatePackCommand =>
        new Command("pack", "repack the given NuPkg at the given version as Unity package")
        {
            PackageIdVersionArgument,
            IncludePrereleaseOption,
            SourceRepositoriesOption,
            TargetRegistryOption,
            OutputDirectoryOption,
            UpmUnityVersionOption,
            UpmPrereleaseSuffixOption,
            UpmBuildmetaSuffixOption,
            UpmRepositoryUrlOption,
            UpmDirectoryUrlOption,
        }.WithHandler(CommandHandler.Create(AmalgamatePack));

    private async Task<int> AmalgamatePack(
        string packageIdVersion,
        bool preRelease,
        IEnumerable<Uri> sources,
        Uri target,
        DirectoryInfo outputDirectory,
        string unity,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        string? repository,
        string? directory,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(nameof(AmalgamatePack));
        Assert.NotNull(Configuration);
        using var context = new Amalgamate.Context(
            configuration: Configuration!,
            sources: sources,
            minUnityVersion: unity,
            target: target,
            repository: repository,
            directory: directory,
            console: console,
            loggerFactory: MainLoggerFactory
        );
        var tuple = await context.PackUpmPackage(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            prereleaseSuffix: prereleaseSuffix,
            buildmetaSuffix: buildmetaSuffix,
            cancellationToken: cancellationToken
        );

        if (tuple is null)
        {
            Logger.LogError("failed to pack amalgamated UPM package for {0}", packageIdVersion);
            return 1;
        }

        var (packageIdentifier, package) = tuple!;
        using (package)
        {
            // write output package.tar.gz
            Logger.LogInformation($"writing package {packageIdentifier}.tgz");
            await package.WriteToTarGzAsync(Path.Join(outputDirectory.FullName, $"{packageIdentifier}.tgz"));
        }

        Logger.LogTrace("exit {0} without error", nameof(AmalgamatePack));
        return 0;
    }
}
