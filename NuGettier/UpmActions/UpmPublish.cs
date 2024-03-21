using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NuGettier;

public partial class NuGettierService
{
    private Command UpmPublishCommand =>
        new Command("publish", "repack the given NuPkg at the given version as Unity package and publish")
        {
            PackageIdVersionArgument,
            IncludePrereleaseOption,
            SourceRepositoriesOption,
            TargetRegistryOption,
            UpmUnityVersionOption,
            UpmPrereleaseSuffixOption,
            UpmBuildmetaSuffixOption,
            UpmRepositoryUrlOption,
            UpmDirectoryUrlOption,
            UpmTokenOption,
            UpmNpmrcOption,
            UpmDryRunOption,
            UpmTimeOutOption,
            UpmPackageAccessLevel,
        }.WithHandler(CommandHandler.Create(UpmPublish));

    private async Task<int> UpmPublish(
        string packageIdVersion,
        bool preRelease,
        IEnumerable<Uri> sources,
        Uri target,
        string unity,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        string? repository,
        string? directory,
        string? token,
        string? npmrc,
        bool dryRun,
        int timeOut,
        Upm.PackageAccessLevel access,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(nameof(UpmPublish));
        Assert.NotNull(Configuration);
        using var context = new Upm.Context(
            host: Host,
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
        var result = await context.PublishUpmPackage(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            prereleaseSuffix: prereleaseSuffix,
            buildmetaSuffix: buildmetaSuffix,
            token: token,
            npmrc: npmrc,
            dryRun: dryRun,
            timeOut: timeOut,
            packageAccessLevel: access,
            cancellationToken: cancellationToken
        );

        if (result != 0)
        {
            Logger.LogError("failed to publish UPM package for {0}", packageIdVersion);
        }

        Logger.LogTrace("exit {0} command handler with error {1}", nameof(UpmPublish), result);
        return result;
    }
}
