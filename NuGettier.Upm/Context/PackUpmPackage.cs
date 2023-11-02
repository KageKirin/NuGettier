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
using Xunit;

namespace NuGettier.Upm;

using NuRepository = NuGet.Protocol.Core.Types.Repository;

public partial class Context
{
    public async Task<Tuple<string, FileDictionary>?> PackUpmPackage(
        string packageName,
        bool preRelease,
        bool latest,
        string? version,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        CancellationToken cancellationToken
    )
    {
        // build package.json from package information
        var packageJson = await GetPackageJson(
            packageName: packageName,
            preRelease: preRelease,
            latest: latest,
            version: version,
            cancellationToken: cancellationToken
        );

        if (packageJson == null)
            return null;

        // add version suffixes
        if (!string.IsNullOrEmpty(prereleaseSuffix))
            packageJson.Version += $"-{prereleaseSuffix}";
        if (!string.IsNullOrEmpty(buildmetaSuffix))
            packageJson.Version += $"+{buildmetaSuffix}";

        // get package rule
        PackageRule packageRule = GetPackageRule(packageName);
        Assert.NotNull(packageRule);

        // fetch package contents for NuGet
        using var packageStream = await FetchPackage(
            packageName: packageName,
            preRelease: preRelease,
            latest: latest,
            version: version,
            cancellationToken: cancellationToken
        );
        if (packageStream == null)
            return null;

        using PackageArchiveReader packageReader = new(packageStream);
        NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

        var selectedFramework = packageReader.SelectPreferredFramework(
            !string.IsNullOrEmpty(packageRule.Framework) ? new[] { packageRule.Framework } : Frameworks
        );
        Console.WriteLine($"selected framework: {selectedFramework}");

        var files = packageReader.GetFrameworkFiles(selectedFramework);
        files.AddRange(packageReader.GetAdditionalFiles(nuspecReader, renameOriginalMarkdownFiles: true));

        // create & add README
        if (!files.ContainsKey(@"README.md"))
        {
            var license = packageJson.GenerateReadme(originalReadme: packageReader.GetReadme(nuspecReader));
            files.Add(@"README.md", license);
            Console.WriteLine($"--- README\n{Encoding.Default.GetString(files[@"README.md"])}\n---");
        }

        // create & add LICENSE
        if (!files.ContainsKey(@"LICENSE.md"))
        {
            var license = packageJson.GenerateLicense(
                originalLicense: packageReader.GetLicense(nuspecReader),
                copyright: nuspecReader.GetCopyright(),
                copyrightHolder: nuspecReader.GetOwners()
            );
            files.Add(@"LICENSE.md", license);
            Console.WriteLine($"--- LICENSE\n{Encoding.Default.GetString(files[@"LICENSE.md"])}\n---");
        }

        // create & add CHANGELOG
        if (!files.ContainsKey(@"CHANGELOG.md"))
        {
            var changelog = packageJson.GenerateChangelog(nuspecReader.GetReleaseNotes());
            files.Add(@"CHANGELOG.md", changelog);
            Console.WriteLine($"--- CHANGELOG\n{Encoding.Default.GetString(files[@"CHANGELOG.md"])}\n---");
        }

        // add file references to package.json
        packageJson.Files.AddRange(files.Keys);

        // add package.json to FileDictionary
        Assert.False(files.ContainsKey(@"package.json"));
        var packageJsonString = packageJson.ToJson();
        files.Add(@"package.json", packageJsonString);
        Console.WriteLine($"--- PACKAGE.JSON\n{packageJsonString}\n---");

        // add meta files
        files.AddMetaFiles(packageJson.Name);

        var packageIdentifier = $"{packageJson.Name}-{packageJson.Version}";
        return new Tuple<string, FileDictionary>(packageIdentifier, files);
    }
}
