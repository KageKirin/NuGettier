using System;
using System.Collections.Generic;
using System.IO;
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
    private static Command GetCommand =>
        new Command("get", "download the given NuPkg at the given version")
        {
            PackageNameArgument,
            IncludePrereleaseOption,
            RetrieveLatestOption,
            SpecificVersionOption,
            SourceRepositoryOption,
            OutputDirectoryOption,
        }
            .WithValidator(ValidateLatestOrVersion)
            .WithHandler(CommandHandler.Create(Get));

    private static async Task<int> Get(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        Uri source,
        DirectoryInfo outputDirectory,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var context = new Core.Context(source: source, console: console);
        return await context.FetchPackage(
            packageName: packageName,
            preRelease: preRelease,
            latest: latest,
            version: version,
            source: source,
            outputDirectory: outputDirectory,
            console: console,
            cancellationToken: cancellationToken
        );
    }
}
