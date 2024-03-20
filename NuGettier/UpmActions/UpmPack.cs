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
using NuGettier.Core;
using NuGettier.Upm;
using NuGettier.Upm.TarGz;
using Xunit;

namespace NuGettier;

public partial class NuGettierService
{
    private Command UpmPackCommand =>
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
        }.WithHandler(CommandHandler.Create(UpmPack));

    private async Task<int> UpmPack(
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
        using var scope = Logger.TraceLocation().BeginScope(nameof(UpmPack));
        Assert.NotNull(Configuration);
        using var context = new Upm.Context(
            configuration: Configuration,
            loggerFactory: MainLoggerFactory,
            logger: MainLoggerFactory.CreateLogger<Upm.Context>(),
            console: console,
            sources: sources,
            minUnityVersion: unity,
            target: target,
            repository: repository,
            directory: directory
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
            Logger.LogError("failed to pack UPM package for {0}", packageIdVersion);
            return 1;
        }

        var (packageIdentifier, package) = tuple!;
        using (package)
        {
            // write output package.tar.gz
            Logger.LogInformation($"writing package {packageIdentifier}.tgz");
            await package.WriteToTarGzAsync(Path.Join(outputDirectory.FullName, $"{packageIdentifier}.tgz"));
        }

        Logger.LogTrace("exit {0} without error", nameof(UpmPack));
        return 0;
    }
}
