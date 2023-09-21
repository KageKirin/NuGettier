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
    static readonly string[] DefaultFrameworks = new[]
    {
        // by order of preference
        "netstandard2.1",
        "netstandard2.0",
        "netstandard1.2",
        "netstandard1.1",
        "netstandard1.0",
    };

    public async Task<Tuple<string, FileDictionary>?> PackUpmPackage(
        string packageName,
        bool preRelease,
        bool latest,
        string? version,
        string? framework,
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

            framework = packageReader.SelectPreferredFramework(
                framework != null ? new[] { framework } : DefaultFrameworks
            );

            var files = packageReader.GetFrameworkFiles(framework);

            // create & add README
            var readme = nuspecReader.GenerateUpmReadme(assemblyName);
            files.Add("README.md", readme);
            Console.WriteLine($"--- README\n{readme}\n---");

            // create & add LICENSE
            var license = nuspecReader.GenerateUpmLicense();
            files.Add("LICENSE.md", license);
            Console.WriteLine($"--- LICENSE\n{license}\n---");

            // create & add CHANGELOG
            var changelog = nuspecReader.GenerateUpmChangelog();
            files.Add("CHANGELOG.md", changelog);
            Console.WriteLine($"--- CHANGELOG\n{changelog}\n---");

            // create package.json
            var packageJson = nuspecReader.GenerateUpmPackageJson(
                framework: framework,
                targetRegistry: target,
                assemblyName: assemblyName
            );

            // add file references to package.json
            packageJson.Files.AddRange(files.Keys);

            // add package.json
            files.Add("package.json", packageJson.ToJson());

            // add meta files
            files.AddMetaFiles(packageJson.Name);

            var packageIdentifier = $"{packageJson.Name}.{packageJson.Version}";
            return new Tuple<string, FileDictionary>(packageIdentifier, files);
        }

        return null;
    }
}
