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
        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3($"{source.ToString()}");

        FindPackageByIdResource resource =
            await repository.GetResourceAsync<FindPackageByIdResource>();
        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
            packageName,
            cache,
            NullLogger.Instance,
            cancellationToken
        );

        NuGetVersion? packageVersion = null;
        if (latest)
        {
            packageVersion = versions.Last();
        }
        else
        {
            packageVersion = new NuGetVersion(version);
        }

        if (
            packageVersion != null
            && await resource.DoesPackageExistAsync(
                packageName,
                packageVersion!,
                cache,
                NullLogger.Instance,
                cancellationToken
            )
        )
        {
            using MemoryStream packageStream = new MemoryStream();

            await resource.CopyNupkgToStreamAsync(
                packageName,
                packageVersion!,
                packageStream,
                cache,
                NullLogger.Instance,
                cancellationToken
            );

            using PackageArchiveReader packageReader = new PackageArchiveReader(packageStream);
            NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

            Console.WriteLine($"Tags: {nuspecReader.GetTags()}");
            Console.WriteLine($"Description: {nuspecReader.GetDescription()}");

            Console.WriteLine($"ID: {nuspecReader.GetId()}");

            Console.WriteLine($"Version: {nuspecReader.GetVersion()}");
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
        }

        return 0;
    }
}
