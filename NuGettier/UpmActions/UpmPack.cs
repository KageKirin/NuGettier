using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
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

namespace NuGettier;

public static partial class Program
{
    private static Command UpmPackCommand =>
        new Command("pack", "repack the given NuPkg at the given version as Unity package")
        {
            PackageNameArgument,
            IncludePrereleaseOption,
            RetrieveLatestOption,
            SpecificVersionOption,
            FrameworkOption,
            SourceRepositoryOption,
            SourceRepositoryUsernameOption,
            SourceRepositoryPasswordOption,
            TargetRegistryOption,
            OutputDirectoryOption,
            UpmPrereleaseSuffixOption,
            UpmBuildmetaSuffixOption,
        }.WithHandler(CommandHandler.Create(UpmPack));

    private static async Task<int> UpmPack(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        string framework,
        Uri source,
        string? username,
        string? password,
        Uri target,
        DirectoryInfo outputDirectory,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var context = new Upm.Context(
            source: source,
            username: username,
            password: password,
            target: target,
            console: console
        );
        var tuple = await context.PackUpmPackage(
            packageName: packageName,
            preRelease: preRelease,
            latest: latest,
            version: version,
            framework: framework,
            prereleaseSuffix: prereleaseSuffix,
            buildmetaSuffix: buildmetaSuffix,
            cancellationToken: cancellationToken
        );

        if (tuple != null)
        {
            var (packageIdentifier, package) = tuple!;
            using (package)
            {
                // write output package.tar.gz
                Console.WriteLine($"writing package {packageIdentifier}.tgz");
                await package.WriteToTarGzAsync(Path.Join(outputDirectory.FullName, $"{packageIdentifier}.tgz"));
                return 0;
            }
        }
        return 1;
    }
}
