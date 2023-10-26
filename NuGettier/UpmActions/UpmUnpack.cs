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
    private static Command UpmUnpackCommand =>
        new Command("unpack", "same as `upm pack`, but writing the unpacked files to the output directory")
        {
            PackageNameArgument,
            IncludePrereleaseOption,
            RetrieveLatestOption,
            SpecificVersionOption,
            FrameworkOption,
            SourceRepositoriesOption,
            TargetRegistryOption,
            OutputDirectoryOption,
            UpmPrereleaseSuffixOption,
            UpmBuildmetaSuffixOption,
        }.WithHandler(CommandHandler.Create(UpmUnpack));

    private static async Task<int> UpmUnpack(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        string framework,
        IEnumerable<Uri> sources,
        Uri target,
        DirectoryInfo outputDirectory,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var context = new Upm.Context(source: sources.First(), target: target, console: console);
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
                Console.WriteLine($"writing unpacked package {packageIdentifier}");
                await package.WriteToDirectoryAsync(Path.Join(outputDirectory.FullName, $"{packageIdentifier}"));
                return 0;
            }
        }
        return 1;
    }
}
