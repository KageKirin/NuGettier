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
            SourceRepositoriesOption,
            OutputDirectoryOption,
        }
            .WithValidator(ValidateLatestOrVersion)
            .WithHandler(CommandHandler.Create(Get));

    private static async Task<int> Get(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        IEnumerable<Uri> sources,
        DirectoryInfo outputDirectory,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var context = new Core.Context(sources: sources, console: console);
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
            Console.WriteLine($"Summary: {nuspecReader.GetSummary()}");
            Console.WriteLine($"Copyright: {nuspecReader.GetCopyright()}");
            Console.WriteLine($"Version: {nuspecReader.GetVersion()}");
            Console.WriteLine($"Tags: {nuspecReader.GetTags()}");
            Console.WriteLine($"Readme: {nuspecReader.GetReadme()}");
            Console.WriteLine($"Icon: {nuspecReader.GetIcon()}");
            Console.WriteLine($"Description: {nuspecReader.GetDescription()}");
            Console.WriteLine($"Authors: {nuspecReader.GetAuthors()}");
            Console.WriteLine($"Owners: {nuspecReader.GetOwners()}");
            Console.WriteLine($"ReleaseNotes: {nuspecReader.GetReleaseNotes()}");
            Console.WriteLine($"Project Url: {nuspecReader.GetProjectUrl()}");
            Console.WriteLine($"LicenseUrl: {nuspecReader.GetLicenseUrl()}");
            Console.WriteLine($"Icon Url: {nuspecReader.GetIconUrl()}");
            Console.WriteLine($"Language: {nuspecReader.GetLanguage()}");
            Console.WriteLine($"LicenseMetadata: {nuspecReader.GetLicenseMetadata()}");
            Console.WriteLine($"RepositoryMetadata: {nuspecReader.GetRepositoryMetadata()}");

            Console.WriteLine("Dependencies:");
            foreach (var dependencyGroup in nuspecReader.GetDependencyGroups())
            {
                Console.WriteLine($"- {dependencyGroup.TargetFramework.GetShortFolderName()}:");
                foreach (var dependency in dependencyGroup.Packages)
                {
                    Console.WriteLine($"  - {dependency.ToString()}");
                }
            }

            Console.WriteLine("Framework Assembly Groups:");
            foreach (var group in nuspecReader.GetFrameworkAssemblyGroups())
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
            foreach (var group in nuspecReader.GetReferenceGroups())
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
            foreach (var file in nuspecReader.GetContentFiles())
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
