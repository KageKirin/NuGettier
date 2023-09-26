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

const string PACKAGE = "System.Text.Json";

ILogger logger = NullLogger.Instance;
CancellationToken cancellationToken = CancellationToken.None;

SourceCacheContext cache = new SourceCacheContext();
SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

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
PackageMetadataResource packageMetadataResource = await repository.GetResourceAsync<PackageMetadataResource>();
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
FindPackageByIdResource packageIdResource = await repository.GetResourceAsync<FindPackageByIdResource>();
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
PackageSearchResource packageSearchResource = await repository.GetResourceAsync<PackageSearchResource>();
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
FindPackageByIdResource findPackageByIdResource = await repository.GetResourceAsync<FindPackageByIdResource>();

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
NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

Console.WriteLine($"Tags: {nuspecReader.GetTags()}");
Console.WriteLine($"Description: {nuspecReader.GetDescription()}");
#endregion


#region read package
Console.WriteLine($"ID: {nuspecReader.GetId()}");

Console.WriteLine($"Version: {nuspecReader.GetVersion()}");
Console.WriteLine($"Description: {nuspecReader.GetDescription()}");
Console.WriteLine($"Authors: {nuspecReader.GetAuthors()}");

Console.WriteLine("Dependencies:");
foreach (var dependencyGroup in nuspecReader.GetDependencyGroups())
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
