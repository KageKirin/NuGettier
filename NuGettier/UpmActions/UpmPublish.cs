using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace NuGettier;

public static partial class Program
{
    private static Command UpmPublishCommand =>
        new Command("publish", "repack the given NuPkg at the given version as Unity package and publish")
        {
            PackageNameArgument,
            IncludePrereleaseOption,
            RetrieveLatestOption,
            SpecificVersionOption,
            FrameworkOption,
            SourceRepositoryOption,
            TargetRegistryOption,
            UpmPrereleaseSuffixOption,
            UpmBuildmetaSuffixOption,
            UpmToken,
            UpmDryRun,
            UpmPackageAccessLevel,
        }.WithHandler(CommandHandler.Create(UpmPublish));

    private static async Task<int> UpmPublish(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        string framework,
        Uri source,
        Uri target,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        string token,
        bool dryRun,
        Upm.PackageAccessLevel access,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var context = new Upm.Context(source: source, target: target, console: console);
        var result = await context.PublishUpmPackage(
            packageName: packageName,
            preRelease: preRelease,
            latest: latest,
            version: version,
            framework: framework,
            prereleaseSuffix: prereleaseSuffix,
            buildmetaSuffix: buildmetaSuffix,
            token: token,
            dryRun: dryRun,
            packageAccessLevel: access,
            cancellationToken: cancellationToken
        );

        if (result != 0)
        {
            console.Error.WriteLine($"publishing failed");
        }

        return result;
    }
}
