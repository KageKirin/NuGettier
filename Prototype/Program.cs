using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

const string PACKAGE = "System.Text.Json";

ILogger logger = NullLogger.Instance;
CancellationToken cancellationToken = CancellationToken.None;

SourceCacheContext cache = new SourceCacheContext();
SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile(@"appsettings.json", optional: true, reloadOnChange: false) //works
    .AddXmlFile(@"appsettings.xml", optional: true, reloadOnChange: false) //works, supersedes ↑
    .AddIniFile(@"appsettings.ini", optional: true, reloadOnChange: false) //works, supersedes ↑
    .AddYamlFile(@"appsettings.yml", optional: true, reloadOnChange: false) //works, supersedes ↑
    .AddTomlFile(@"appsettings.toml", optional: true, reloadOnChange: false) //works, supersedes ↑
    .AddEnvironmentVariables()
    .AddCommandLine(args);

IConfiguration configuration = configurationBuilder.Build();

AppSettings appSettings = new(configuration);
Console.WriteLine($"appSettings.AppName: {appSettings.AppName}");
Console.WriteLine($"appSettings.AppVersion: {appSettings.AppVersion}");

#region settings
// Load machine and user settings
var settings = Settings.LoadDefaultSettings(null);

foreach (var file in settings.GetConfigFilePaths())
{
    Console.WriteLine($"config: {file}");
}

// Extract some data from the settings
string globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(settings);
var sources = SettingsUtility.GetEnabledSources(settings);
foreach (var source in sources)
{
    Console.WriteLine($"source: {source.Name} -> {source.Source} {source.Description}");
}
#endregion


#region metadata
PackageMetadataResource packageMetadataResource = await repository.GetResourceAsync<PackageMetadataResource>(
    cancellationToken
);
IEnumerable<IPackageSearchMetadata> packages = await packageMetadataResource.GetMetadataAsync(
    PACKAGE,
    includePrerelease: false,
    includeUnlisted: false,
    cache,
    logger,
    cancellationToken
);

foreach (IPackageSearchMetadata package in packages)
{
    Console.WriteLine($"Version: {package.Identity.Version}");
    Console.WriteLine($"Listed: {package.IsListed}");
    Console.WriteLine($"Tags: {package.Tags}");
    Console.WriteLine($"Description: {package.Description}");
}
#endregion

#region versions
FindPackageByIdResource packageIdResource = await repository.GetResourceAsync<FindPackageByIdResource>(
    cancellationToken
);
IEnumerable<NuGetVersion> versions = await packageIdResource.GetAllVersionsAsync(
    PACKAGE,
    cache,
    logger,
    cancellationToken
);

foreach (NuGetVersion version in versions)
{
    Console.WriteLine($"Found version {version}");
}
#endregion

#region search packages
PackageSearchResource packageSearchResource = await repository.GetResourceAsync<PackageSearchResource>(
    cancellationToken
);
SearchFilter searchFilter = new SearchFilter(includePrerelease: true);

IEnumerable<IPackageSearchMetadata> results = await packageSearchResource.SearchAsync(
    "json",
    searchFilter,
    skip: 0,
    take: 20,
    logger,
    cancellationToken
);

foreach (IPackageSearchMetadata result in results)
{
    Console.WriteLine($"Found package {result.Identity.Id} {result.Identity.Version}");
}
#endregion

#region download package
FindPackageByIdResource findPackageByIdResource = await repository.GetResourceAsync<FindPackageByIdResource>(
    cancellationToken
);

NuGetVersion packageVersion = new NuGetVersion("7.0.3");
using MemoryStream packageStream = new MemoryStream();

await findPackageByIdResource.CopyNupkgToStreamAsync(
    PACKAGE,
    packageVersion,
    packageStream,
    cache,
    logger,
    cancellationToken
);

Console.WriteLine($"Downloaded package {PACKAGE} {packageVersion}");

using PackageArchiveReader packageReader = new PackageArchiveReader(packageStream);

Console.WriteLine($"Tags: {packageReader.NuspecReader.GetTags()}");
Console.WriteLine($"Description: {packageReader.NuspecReader.GetDescription()}");
#endregion


#region read package
Console.WriteLine($"ID: {packageReader.NuspecReader.GetId()}");

Console.WriteLine($"Version: {packageReader.NuspecReader.GetVersion()}");
Console.WriteLine($"Description: {packageReader.NuspecReader.GetDescription()}");
Console.WriteLine($"Authors: {packageReader.NuspecReader.GetAuthors()}");

Console.WriteLine("Dependencies:");
foreach (var dependencyGroup in packageReader.NuspecReader.GetDependencyGroups())
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
#endregion
