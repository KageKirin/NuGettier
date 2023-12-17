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
    private static Command AmalgamatePublishCommand =>
        new Command("publish", "repack the given NuPkg at the given version as Unity package and publish")
        {
            PackageIdArgument,
            IncludePrereleaseOption,
            RetrieveLatestOption,
            SpecificVersionOption,
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
        }.WithHandler(CommandHandler.Create(AmalgamatePublish));

    private static async Task<int> AmalgamatePublish(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
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
        Assert.NotNull(Configuration);
        using var context = new Amalgamate.Context(
            configuration: Configuration!,
            sources: sources,
            minUnityVersion: unity,
            target: target,
            repository: repository,
            directory: directory,
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
