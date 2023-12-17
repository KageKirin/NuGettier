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
using NuGettier.Core;
using NuGettier.Upm;
using NuGettier.Upm.TarGz;
using Xunit;

namespace NuGettier;

public static partial class Program
{
    private static Command UpmUnpackCommand =>
        new Command("unpack", "same as `upm pack`, but writing the unpacked files to the output directory")
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
        }.WithHandler(CommandHandler.Create(UpmUnpack));

    private static async Task<int> UpmUnpack(
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
        Assert.NotNull(Configuration);
        packageIdVersion.SplitPackageIdVersion(out var packageId, out var version, out var latest);
        using var context = new Upm.Context(
            configuration: Configuration!,
            sources: sources,
            minUnityVersion: unity,
            target: target,
            repository: repository,
            directory: directory,
            console: console
        );
        var tuple = await context.PackUpmPackage(
            packageId: packageId,
            preRelease: preRelease,
            latest: latest,
            version: version,
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
                Console.WriteLine($"writing unpacked package {packageIdentifier}");
                await package.WriteToDirectoryAsync(Path.Join(outputDirectory.FullName, $"{packageIdentifier}"));
                return 0;
            }
        }
        return 1;
    }
}
