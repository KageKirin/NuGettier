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
            TargetRegistryOption,
            OutputDirectoryOption,
        }.WithHandler(CommandHandler.Create(UpmPack));

    private static async Task<int> UpmPack(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        string framework,
        Uri source,
        Uri target,
        DirectoryInfo outputDirectory,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var context = new Upm.Context(source: source, target: target, console: console);
        return await context.PackUpmPackage(
            packageName: packageName,
            preRelease: preRelease,
            latest: latest,
            version: version,
            framework: framework,
            source: source,
            target: target,
            outputDirectory: outputDirectory,
            console: console,
            cancellationToken: cancellationToken
        );
    }
}
