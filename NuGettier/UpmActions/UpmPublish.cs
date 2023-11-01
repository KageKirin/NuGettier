using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Xunit;

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
            SourceRepositoriesOption,
            TargetRegistryOption,
            UpmPrereleaseSuffixOption,
            UpmBuildmetaSuffixOption,
            UpmRepositoryUrlOption,
            UpmTokenOption,
            UpmNpmrcOption,
            UpmDryRunOption,
            UpmPackageAccessLevel,
        }.WithHandler(CommandHandler.Create(UpmPublish));

    private static async Task<int> UpmPublish(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        IEnumerable<Uri> sources,
        Uri target,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        string? repository,
        string? token,
        string? npmrc,
        bool dryRun,
        Upm.PackageAccessLevel access,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        Assert.NotNull(Configuration);
        using var context = new Upm.Context(
            configuration: Configuration!,
            sources: sources,
            target: target,
            repository: repository,
            console: console
        );
        var result = await context.PublishUpmPackage(
            packageName: packageName,
            preRelease: preRelease,
            latest: latest,
            version: version,
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
            console.Error.WriteLine($"publishing failed");
        }

        return result;
    }
}
