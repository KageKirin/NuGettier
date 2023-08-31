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
    public async Task<Package?> PackUpmPackage(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        string framework,
        CancellationToken cancellationToken
    )
    {
        using var packageStream = await FetchPackage(
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

            // create the Package
            Package package = new(packageName);

            // get nugettier tool name+version
            var executingAssembly = Assembly.GetEntryAssembly();
            var assemblyName = executingAssembly.GetName();

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
            package.Add(readme);
            Console.WriteLine($"---\n{readme}\n---");

            // create package.json data (serialization happens later)
            Upm.PackageJson packageJson =
                new()
                {
                    // TODO: naming convention => separate function. use also on dependencies
                    Name = $"com.{nuspecReader.GetAuthors()}.{nuspecReader.GetId()}"
                        .ToLowerInvariant()
                        .Replace(@" ", @""),
                    Version = nuspecReader.GetVersion().ToString(),
                    Keywords = string.IsNullOrWhiteSpace(nuspecReader.GetTags())
                        ? new List<string>()
                        : nuspecReader.GetTags().Split(@" ").ToList(),
                    Description = nuspecReader.GetDescription(),
                    DisplayName = // TODO: separate function
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
            // TODO: fetch more information (author)
            // TODO: generate name for dependency
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
            // TODO: standalone filter function
            // -> framework dll/xml
            // -> LICENSE*
            // TODO: patch package.json/.files[] accordingly
            // TODO: whitelist/blacklist
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
                package.Add(file, ms.GetBuffer());
            }

            // add package.json
            console.WriteLine($"---\n{packageJson.ToJson()}\n---");
            package.Add(packageJson);

            // for all entries in tarDictionary, generate and add Meta files
            foreach (
                var file in package.Files.Keys
                    .Where(file => Path.GetExtension(file) != @".meta")
                    .ToList()
            )
            {
                package.Add($"{file}.meta", Upm.MetaGen.GenerateMeta(packageJson.Name, file));
            }

            return package;
        }
        return null;
    }
}
