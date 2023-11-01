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
using Xunit;

namespace NuGettier;

public static partial class Program
{
    private static Command UpmUnpackCommand =>
        new Command("unpack", "same as `upm pack`, but writing the unpacked files to the output directory")
        {
            PackageNameArgument,
            IncludePrereleaseOption,
            RetrieveLatestOption,
            SpecificVersionOption,
            SourceRepositoriesOption,
            TargetRegistryOption,
            OutputDirectoryOption,
            UpmPrereleaseSuffixOption,
            UpmBuildmetaSuffixOption,
            UpmRepositoryUrlOption,
        }.WithHandler(CommandHandler.Create(UpmUnpack));

    private static async Task<int> UpmUnpack(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        IEnumerable<Uri> sources,
        Uri target,
        DirectoryInfo outputDirectory,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        string? repository,
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
        var tuple = await context.PackUpmPackage(
            packageName: packageName,
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
