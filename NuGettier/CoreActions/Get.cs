using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGettier.Core;
using Xunit;

namespace NuGettier;

public partial class NuGettierService
{
    private Command GetCommand =>
        new Command("get", "download the given NuPkg at the given version")
        {
            PackageIdVersionArgument,
            IncludePrereleaseOption,
            SourceRepositoriesOption,
            OutputDirectoryOption,
        }.WithHandler(CommandHandler.Create(Get));

    private async Task<int> Get(
        string packageIdVersion,
        bool preRelease,
        IEnumerable<Uri> sources,
        DirectoryInfo outputDirectory,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(nameof(Get));
        Assert.NotNull(Configuration);
        using var context = new Core.Context(
            configuration: Configuration,
            loggerFactory: MainLoggerFactory,
            logger: MainLoggerFactory.CreateLogger<Core.Context>(),
            console: console,
            sources: sources
        );
        using var packageStream = await context.FetchPackage(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );

        if (packageStream is null)
        {
            Logger.LogError("failed to fetch package for {0}", packageIdVersion);
            return 1;
        }

        using (PackageArchiveReader packageReader = new PackageArchiveReader(packageStream))
        {
            Console.WriteLine($"ID: {packageReader.NuspecReader.GetId()}");
            Console.WriteLine($"Title: {packageReader.NuspecReader.GetTitle()}");
            Console.WriteLine($"Summary: {packageReader.NuspecReader.GetSummary()}");
            Console.WriteLine($"Copyright: {packageReader.NuspecReader.GetCopyright()}");
            Console.WriteLine($"Version: {packageReader.NuspecReader.GetVersion()}");
            Console.WriteLine($"Tags: {packageReader.NuspecReader.GetTags()}");
            Console.WriteLine($"Readme: {packageReader.NuspecReader.GetReadme()}");
            Console.WriteLine($"Icon: {packageReader.NuspecReader.GetIcon()}");
            Console.WriteLine($"Description: {packageReader.NuspecReader.GetDescription()}");
            Console.WriteLine($"Authors: {packageReader.NuspecReader.GetAuthors()}");
            Console.WriteLine($"Owners: {packageReader.NuspecReader.GetOwners()}");
            Console.WriteLine($"ReleaseNotes: {packageReader.NuspecReader.GetReleaseNotes()}");
            Console.WriteLine($"Project Url: {packageReader.NuspecReader.GetProjectUrl()}");
            Console.WriteLine($"LicenseUrl: {packageReader.NuspecReader.GetLicenseUrl()}");
            Console.WriteLine($"Icon Url: {packageReader.NuspecReader.GetIconUrl()}");
            Console.WriteLine($"Language: {packageReader.NuspecReader.GetLanguage()}");
            Console.WriteLine($"LicenseMetadata: {packageReader.NuspecReader.GetLicenseMetadata()}");
            Console.WriteLine($"RepositoryMetadata: {packageReader.NuspecReader.GetRepositoryMetadata()}");

            Console.WriteLine("Dependencies:");
            foreach (var dependencyGroup in packageReader.NuspecReader.GetDependencyGroups())
            {
                Console.WriteLine($"- {dependencyGroup.TargetFramework.GetShortFolderName()}:");
                foreach (var dependency in dependencyGroup.Packages)
                {
                    Console.WriteLine($"  - {dependency.ToString()}");
                }
            }

            Console.WriteLine("Framework Assembly Groups:");
            foreach (var group in packageReader.NuspecReader.GetFrameworkAssemblyGroups())
            {
                Console.WriteLine($"- TargetFramework: {group.TargetFramework.ToString()}");
                Console.WriteLine($"  HasEmptyFolder: {group.HasEmptyFolder}");
                Console.WriteLine($"  Items:");
                foreach (var item in group.Items)
                {
                    Console.WriteLine($"  - {item}");
                }
            }

            Console.WriteLine("Reference Groups:");
            foreach (var group in packageReader.NuspecReader.GetReferenceGroups())
            {
                Console.WriteLine($"- TargetFramework: {group.TargetFramework.ToString()}");
                Console.WriteLine($"  HasEmptyFolder: {group.HasEmptyFolder}");
                Console.WriteLine($"  Items:");
                foreach (var item in group.Items)
                {
                    Console.WriteLine($"  - {item}");
                }
            }

            // not very interesting (empty)
            Console.WriteLine("Content Files:");
            foreach (var file in packageReader.NuspecReader.GetContentFiles())
            {
                Console.WriteLine($"- file:");
                Console.WriteLine($"  Include: {file.Include}");
                Console.WriteLine($"  Exclude: {file.Exclude}");
                Console.WriteLine($"  BuildAction: {file.BuildAction}");
                Console.WriteLine($"  CopyToOutput: {file.CopyToOutput}");
                Console.WriteLine($"  Flatten: {file.Flatten}");
            }

            Console.WriteLine("Files:");
            foreach (var file in packageReader.GetFiles())
            {
                Console.WriteLine($" - {file}");
            }

            //Console.WriteLine($"{JsonSerializer.Serialize(packageReader, new JsonSerializerOptions(){ReferenceHandler = ReferenceHandler.Preserve})}");
            //Console.WriteLine($"{JsonSerializer.Serialize(packageReader.NuspecReader, new JsonSerializerOptions(){ReferenceHandler = ReferenceHandler.Preserve})}");

            if (!outputDirectory.Exists)
            {
                outputDirectory.Create();
            }

            using FileStream fileStream = new FileStream(
                $"{Path.Join(outputDirectory.FullName, $"{packageReader.NuspecReader.GetId()}-{packageReader.NuspecReader.GetVersion()}.nupkg")}",
                FileMode.Create,
                FileAccess.Write
            );
            packageStream.WriteTo(fileStream);
        }

        Logger.LogTrace("exit {0} without error", nameof(Get));
        return 0;
    }
}
