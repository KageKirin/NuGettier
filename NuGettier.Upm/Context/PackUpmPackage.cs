using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
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
using NuGettier.Upm.TarGz;
using Xunit;

namespace NuGettier.Upm;

public partial class Context
{
    public virtual async Task<Tuple<string, FileDictionary>?> PackUpmPackage(
        string packageIdVersion,
        bool preRelease,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        CancellationToken cancellationToken
    )
    {
        // build package.json from package information
        var packageJson = await GetPackageJson(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );

        if (packageJson == null)
            return null;

        // add version suffixes
        if (!string.IsNullOrEmpty(prereleaseSuffix))
            packageJson.Version += $"-{prereleaseSuffix}";
        if (!string.IsNullOrEmpty(buildmetaSuffix))
            packageJson.Version += $"+{buildmetaSuffix}";

        // fetch package contents for NuGet
        using var packageStream = await FetchPackage(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );
        if (packageStream == null)
            return null;

        using PackageArchiveReader packageReader = new(packageStream);
        FileDictionary files = await GetPackageFiles(packageReader, NugetFramework, cancellationToken);

        // create & add README
        if (!files.ContainsKey(@"README.md"))
        {
            var readme = packageJson.GenerateReadme(
                originalReadme: packageReader.GetReadme(),
                readmeFactory: new ReadmeFactory(LoggerFactory)
            );
            files.Add(@"README.md", readme);
            Logger.LogDebug($"--- README\n{Encoding.Default.GetString(files[@"README.md"])}\n---");
        }

        // create & add LICENSE
        if (!files.ContainsKey(@"LICENSE.md"))
        {
            var license = packageJson.GenerateLicense(
                originalLicense: packageReader.GetLicense(),
                copyright: packageReader.NuspecReader.GetCopyright(),
                copyrightHolder: packageReader.NuspecReader.GetOwners(),
                licenseFactory: new LicenseFactory(LoggerFactory)
            );
            files.Add(@"LICENSE.md", license);
            Logger.LogDebug($"--- LICENSE\n{Encoding.Default.GetString(files[@"LICENSE.md"])}\n---");
        }

        // create & add CHANGELOG
        if (!files.ContainsKey(@"CHANGELOG.md"))
        {
            var changelog = packageJson.GenerateChangelog(
                releaseNotes: packageReader.NuspecReader.GetReleaseNotes(),
                changelogFactory: new ChangelogFactory(LoggerFactory)
            );
            files.Add(@"CHANGELOG.md", changelog);
            Logger.LogDebug($"--- CHANGELOG\n{Encoding.Default.GetString(files[@"CHANGELOG.md"])}\n---");
        }

        // add file references to package.json
        packageJson.Files.AddRange(files.Keys);

        // add package.json to FileDictionary
        Assert.False(files.ContainsKey(@"package.json"));
        var packageJsonString = packageJson.ToJson();
        files.Add(@"package.json", packageJsonString);
        Logger.LogDebug($"--- PACKAGE.JSON\n{packageJsonString}\n---");

        // add meta files
        files.AddMetaFiles(seed: packageJson.Name, metaFactory: new MetaFactory(LoggerFactory));

        var packageIdentifier = $"{packageJson.Name}-{packageJson.Version}";
        return new Tuple<string, FileDictionary>(packageIdentifier, files);
    }
}
