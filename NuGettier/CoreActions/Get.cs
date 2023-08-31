using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        using var packageStream = await context.FetchPackage(
            packageName: packageName,
            preRelease: preRelease,
            latest: latest,
            version: version,
            cancellationToken: cancellationToken
        );

        if (packageStream != null)
        {
            using PackageArchiveReader packageReader = new PackageArchiveReader(packageStream);
            NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

            Console.WriteLine($"ID: {nuspecReader.GetId()}");
            Console.WriteLine($"Title: {nuspecReader.GetTitle()}");
            Console.WriteLine($"Version: {nuspecReader.GetVersion()}");
            Console.WriteLine($"Tags: {nuspecReader.GetTags()}");
            Console.WriteLine($"Description: {nuspecReader.GetDescription()}");
            Console.WriteLine($"Authors: {nuspecReader.GetAuthors()}");

            Console.WriteLine("Dependencies:");
            foreach (var dependencyGroup in nuspecReader.GetDependencyGroups())
            {
                Console.WriteLine($" - {dependencyGroup.TargetFramework.GetShortFolderName()}");
                foreach (var dependency in dependencyGroup.Packages)
                {
                    Console.WriteLine($"   > {dependency.Id} {dependency.VersionRange}");
                }
            }

            Console.WriteLine("Files:");
            foreach (var file in packageReader.GetFiles())
            {
                Console.WriteLine($" - {file}");
            }

            //Console.WriteLine($"{JsonSerializer.Serialize(packageReader, new JsonSerializerOptions(){ReferenceHandler = ReferenceHandler.Preserve})}");
            //Console.WriteLine($"{JsonSerializer.Serialize(nuspecReader, new JsonSerializerOptions(){ReferenceHandler = ReferenceHandler.Preserve})}");

            if (!outputDirectory.Exists)
            {
                outputDirectory.Create();
            }

            using (
                FileStream fileStream = new FileStream(
                    $"{Path.Join(outputDirectory.FullName, $"{packageName}-{nuspecReader.GetVersion()}.nupkg")}",
                    FileMode.Create,
                    FileAccess.Write
                )
            )
            {
                packageStream.WriteTo(fileStream);
            }
            return 0;
        }

        return 1;
    }
}
