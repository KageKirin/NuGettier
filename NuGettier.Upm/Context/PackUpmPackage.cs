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
    public async Task<Tuple<string, FileDictionary>?> PackUpmPackage(
        string packageName,
        bool preRelease,
        bool latest,
        string? version,
        string? framework,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
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

            // get nugettier tool name+version
            // required for README, PackageJson
            var executingAssembly = Assembly.GetEntryAssembly();
            var assemblyName = executingAssembly.GetName();

            var selectedFramework = packageReader.SelectPreferredFramework(
                framework != null ? new[] { framework } : DefaultFrameworks
            );
            Console.WriteLine($"selected framework: {selectedFramework}");

            var files = packageReader.GetFrameworkFiles(selectedFramework);
            files.AddRange(packageReader.GetAdditionalFiles(nuspecReader, renameOriginalMarkdownFiles: true));

            // create & add README
            var readme = nuspecReader.GenerateUpmReadme(assemblyName, prereleaseSuffix, buildmetaSuffix);
            if (!files.ContainsKey(@"README.md"))
            {
                files.Add(@"README.md", readme);
                Console.WriteLine($"--- README\n{Encoding.Default.GetString(files[@"README.md"])}\n---");
            }

            // create & add LICENSE
            var license = nuspecReader.GenerateUpmLicense(prereleaseSuffix, buildmetaSuffix);
            if (!files.ContainsKey(@"LICENSE.md"))
            {
                files.Add(@"LICENSE.md", license);
                Console.WriteLine($"--- LICENSE\n{Encoding.Default.GetString(files[@"LICENSE.md"])}\n---");
            }

            // create & add CHANGELOG
            var changelog = nuspecReader.GenerateUpmChangelog(prereleaseSuffix, buildmetaSuffix);
            if (!files.ContainsKey(@"CHANGELOG.md"))
            {
                files.Add(@"CHANGELOG.md", license);
                Console.WriteLine($"--- CHANGELOG\n{Encoding.Default.GetString(files[@"CHANGELOG.md"])}\n---");
            }

            // create package.json
            var packageJson = nuspecReader.GenerateUpmPackageJson(
                framework: selectedFramework,
                targetRegistry: Target,
                assemblyName: assemblyName,
                async (dependencyPackageName, dependencyPackageVersion) =>
                {
                    var dependencyPackage = await GetPackageInformation(
                        packageName: dependencyPackageName,
                        preRelease: true,
                        latest: false,
                        version: dependencyPackageVersion,
                        cancellationToken: cancellationToken
                    );
                    return ((IPackageSearchMetadata)dependencyPackage!).GetUpmPackageName();
                },
                prereleaseSuffix: prereleaseSuffix,
                buildmetaSuffix: buildmetaSuffix
            );

            // add file references to package.json
            packageJson.Files.AddRange(files.Keys);

            // add package.json
            var packageJsonString = packageJson.ToJson();
            files.Add("package.json", packageJsonString);
            Console.WriteLine($"--- PACKAGE.JSON\n{packageJsonString}\n---");

            // add meta files
            files.AddMetaFiles(packageJson.Name);

            var packageIdentifier = $"{packageJson.Name}-{packageJson.Version}";
            return new Tuple<string, FileDictionary>(packageIdentifier, files);
        }

        return null;
    }
}
