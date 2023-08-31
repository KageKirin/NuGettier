using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Reflection;
using System.CommandLine;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using NuGettier.Upm.TarGz;

namespace NuGettier.Upm;

using NuRepository = NuGet.Protocol.Core.Types.Repository;

public partial class Context
{
    public async Task<int> PackUpmPackage(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        string framework,
        DirectoryInfo outputDirectory,
        CancellationToken cancellationToken
    )
    {
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
            using Upm.TarGz.TarDictionary tarDictionary = new();

            using MemoryStream packageStream = new();
            await resource.CopyNupkgToStreamAsync(
                packageName,
                packageVersion!,
                packageStream,
                cache,
                NullLogger.Instance,
                cancellationToken
            );

            PackageArchiveReader packageReader = new PackageArchiveReader(packageStream);
            NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

            // generate and add README.md
            Upm.Templates.Readme readme =
                new(
                    name: packageName,
                    version: nuspecReader.GetVersion().ToString(),
                    description: nuspecReader.GetDescription()
                )
                {
                    ReleaseNotes = nuspecReader.GetReleaseNotes(),
                };
            var readmeText = readme.ToString();
            Console.WriteLine($"---\n{readmeText}\n---");
            tarDictionary.Add("packages/README.md", Encoding.ASCII.GetBytes(readmeText));

            // create package.json data (serialization happens later)
            var executingAssembly = Assembly.GetEntryAssembly();
            var assemblyName = executingAssembly.GetName();
            Upm.PackageJson packageJson =
                new()
                {
                    Name = $"com.{nuspecReader.GetAuthors()}.{nuspecReader.GetId()}"
                        .ToLowerInvariant()
                        .Replace(@" ", @""),
                    Version = nuspecReader.GetVersion().ToString(),
                    Keywords = string.IsNullOrWhiteSpace(nuspecReader.GetTags())
                        ? new List<string>()
                        : nuspecReader.GetTags().Split(@" ").ToList(),
                    Description = nuspecReader.GetDescription(),
                    DisplayName =
                        (
                            string.IsNullOrWhiteSpace(nuspecReader.GetTitle())
                                ? nuspecReader.GetId()
                                : nuspecReader.GetTitle()
                        )
                        + $" ({framework} DLL) [repacked by {assemblyName.Name} v{assemblyName.Version.ToString()}]",
                    Author = new()
                    {
                        Name = nuspecReader.GetAuthors(),
                        Url = nuspecReader.GetProjectUrl(),
                    },
                    Repository = new() { Url = nuspecReader.GetProjectUrl(), },
                    PublishingConfiguration = new() { Registry = target.ToString(), }
                };
            packageJson.Files.Add(@"**.meta");
            packageJson.Files.Add(@"**.dll");
            packageJson.Files.Add(@"**.xml");
            packageJson.Files.Add(@"**.md");

            Console.WriteLine($"Authors: {nuspecReader.GetAuthors()}");
            //Console.WriteLine($"License: {nuspecReader.GetLicense()}");

            // gather dependencies


            Console.WriteLine("Dependencies:");
            var dependencyGroups = nuspecReader
                .GetDependencyGroups()
                .Where(d => d.TargetFramework.GetShortFolderName() == framework);
            foreach (var dependencyGroup in dependencyGroups)
            {
                foreach (var dependency in dependencyGroup.Packages)
                {
                    Console.WriteLine($"   > {dependency.Id} {dependency.VersionRange}");
                }
            }

            // gather files to pack
            Console.WriteLine("Files:");
            foreach (
                var file in packageReader
                    .GetFiles()
                    .Where(f => Path.GetDirectoryName(f) == Path.Join("lib", framework))
            )
            {
                Console.WriteLine($" - {file} [{Path.GetDirectoryName(file)}]");
                using var stream = packageReader.GetStream(file);
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                Console.WriteLine($" {ms.Length}");
                tarDictionary.Add($"packages/{file}", ms.GetBuffer());
            }

            // generate and add package.json
            var packageJsonText = packageJson.ToJson();
            tarDictionary.Add("packages/package.json", Encoding.ASCII.GetBytes(packageJsonText));
            console.WriteLine($"---\n{packageJsonText}\n---");

            // for all entries in tarDictionary, generate and add Meta files
            foreach (
                var key in tarDictionary.Keys
                    .Where(key => Path.GetExtension(key) != @".meta")
                    .ToList()
            )
            {
                var metaText = Upm.MetaGen.GenerateMeta(packageJson.Name, key);
                tarDictionary.Add($"{key}.meta", Encoding.ASCII.GetBytes(metaText));
                console.WriteLine($"---\n{metaText}\n---");
            }

            // write output package.tar.gz
            tarDictionary.WriteToTarGz(
                Path.Join(outputDirectory.FullName, $"{packageJson.Name}-{packageJson.Version}.tgz")
            );
        }
        return 0;
    }
}
