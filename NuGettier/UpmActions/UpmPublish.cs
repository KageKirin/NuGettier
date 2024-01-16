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

public partial class Program
{
    private static Command UpmPublishCommand =>
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
            UpmPackageAccessLevel,
        }.WithHandler(CommandHandler.Create(UpmPublish));

    private static async Task<int> UpmPublish(
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
        Upm.PackageAccessLevel access,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        Logger.LogTrace("entered {0} command handler", "UpmPublish");
        Assert.NotNull(Configuration);
        using var context = new Upm.Context(
            configuration: Configuration!,
            sources: sources,
            minUnityVersion: unity,
            target: target,
            repository: repository,
            directory: directory,
            console: console,
            loggerFactory: MainLoggerFactory
        );
        var result = await context.PublishUpmPackage(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            prereleaseSuffix: prereleaseSuffix,
            buildmetaSuffix: buildmetaSuffix,
            token: token,
            npmrc: npmrc,
            dryRun: dryRun,
            packageAccessLevel: access,
            cancellationToken: cancellationToken
        );

        if (result != 0)
        {
            Logger.LogError($"publishing failed");
        }

        Logger.LogTrace("exit {0} command handler with error {1}", "UpmPublish", result);
        return result;
    }
}
