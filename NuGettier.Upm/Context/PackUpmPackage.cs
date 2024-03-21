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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        // build package.json from package information
        var packageJson = await GetPackageJson(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            prereleaseSuffix: prereleaseSuffix,
            buildmetaSuffix: buildmetaSuffix,
            cancellationToken: cancellationToken
        );

        if (packageJson == null)
            return null;

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
            using (var serviceScope = Host.Services.CreateScope())
            {
                ReadmeFactory readmeFactory = serviceScope.ServiceProvider.GetRequiredService<ReadmeFactory>();
                var readme = packageJson.GenerateReadme(
                    originalReadme: packageReader.GetReadme(),
                    readmeFactory: readmeFactory
                );
                files.Add(@"README.md", readme);
            }
            Logger.LogDebug("added README.md\n{0}", Encoding.Default.GetString(files[@"README.md"]));
        }

        // create & add LICENSE
        if (!files.ContainsKey(@"LICENSE.md"))
        {
            using (var serviceScope = Host.Services.CreateScope())
            {
                LicenseFactory licenseFactory = serviceScope.ServiceProvider.GetRequiredService<LicenseFactory>();
                var license = packageJson.GenerateLicense(
                    originalLicense: packageReader.GetLicense(),
                    copyright: packageReader.NuspecReader.GetCopyright(),
                    copyrightHolder: packageReader.NuspecReader.GetOwners(),
                    licenseFactory: licenseFactory
                );
                files.Add(@"LICENSE.md", license);
            }
            Logger.LogDebug("added LICENSE.md\n{0}", Encoding.Default.GetString(files[@"LICENSE.md"]));
        }

        // create & add CHANGELOG
        if (!files.ContainsKey(@"CHANGELOG.md"))
        {
            using (ChangelogFactory changelogFactory = new(LoggerFactory))
            {
                var changelog = packageJson.GenerateChangelog(
                    releaseNotes: packageReader.NuspecReader.GetReleaseNotes(),
                    changelogFactory: new ChangelogFactory(LoggerFactory)
                );
                files.Add(@"CHANGELOG.md", changelog);
            }
            Logger.LogDebug("added CHANGELOG.md\n{0}", Encoding.Default.GetString(files[@"CHANGELOG.md"]));
        }

        // add file references to package.json
        packageJson.Files.AddRange(files.Keys);

        // add package.json to FileDictionary
        Assert.False(files.ContainsKey(@"package.json"));
        var packageJsonString = packageJson.ToJson();
        files.Add(@"package.json", packageJsonString);
        Logger.LogDebug("generated package.json:\n{0}", packageJsonString);

        // add meta files
        using (MetaFactory metaFactory = new(seed: packageJson.Name, LoggerFactory))
            files.AddMetaFiles(metaFactory: metaFactory);

        var packageIdentifier = $"{packageJson.Name}-{packageJson.Version}";
        return new Tuple<string, FileDictionary>(packageIdentifier, files);
    }
}
