using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ZLogger;

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
hostBuilder
    .ConfigureAppConfiguration(
        (context, builder) =>
        {
            builder
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(@"appsettings.json", optional: true, reloadOnChange: false) //works
                .AddXmlFile(@"appsettings.xml", optional: true, reloadOnChange: false) //works, supersedes ↑
                .AddIniFile(@"appsettings.ini", optional: true, reloadOnChange: false) //works, supersedes ↑
                .AddYamlFile(@"appsettings.yml", optional: true, reloadOnChange: false) //works, supersedes ↑
                .AddTomlFile(@"appsettings.toml", optional: true, reloadOnChange: false) //works, supersedes ↑
                .AddEnvironmentVariables()
                .AddCommandLine(args);
        }
    )
    .ConfigureLogging(
        (context, builder) =>
        {
            builder
                .AddConfiguration(context.Configuration.GetSection("logging"))
                .AddConsole() //< add console as logging target
                .AddDebug() //< add debug output as logging target
                .AddZLoggerFile(Path.Join(Environment.CurrentDirectory, "prototype.log")) //< add file output as logging target
                .SetMinimumLevel(
#if DEBUG
                    //< set minimum level to trace in Debug
                    Microsoft.Extensions.Logging.LogLevel.Trace
#else
                    //< set minimum level to error in Release
                    Microsoft.Extensions.Logging.LogLevel.Error
#endif
                );
        }
    )
    .ConfigureServices(
        (context, services) =>
        {
            services.AddOptions();
            services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
            services.AddHostedService<SampleWorker>();
            //services.AddSingleton<IHostedService, SampleWorker>();
        }
    );

//await hostBuilder.RunConsoleAsync();
IHost host = hostBuilder.Build();
await host.RunAsync();

internal class SampleWorker : IHostedService, IDisposable
{
    const string PACKAGE = "System.Text.Json";
    private readonly Microsoft.Extensions.Logging.ILogger _logger; //<! conflict with NuGet.ILogger, hence long name
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IHostApplicationLifetime _appLifetime;

    public SampleWorker(
        ILogger<SampleWorker> logger,
        IOptions<AppSettings> appSettings,
        IHostApplicationLifetime appLifetime
    )
    {
        _logger = logger;
        _appSettings = appSettings;
        _appLifetime = appLifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(
            () =>
                Task.Run(async () =>
                {
                    try
                    {
                        SourceCacheContext cache = new SourceCacheContext();
                        SourceRepository repository = Repository.Factory.GetCoreV3(
                            "https://api.nuget.org/v3/index.json"
                        );

                        _logger.ZLogInformation($"appSettings.AppName: {_appSettings.Value.AppName}");
                        _logger.ZLogInformation($"appSettings.AppVersion: {_appSettings.Value.AppVersion}");

                        #region settings
                        // Load machine and user settings
                        var settings = Settings.LoadDefaultSettings(null);
                        foreach (var file in settings.GetConfigFilePaths())
                        {
                            _logger.ZLogInformation($"config: {file}");
                        }

                        // Extract some data from the settings
                        string globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(settings);
                        var sources = SettingsUtility.GetEnabledSources(settings);
                        foreach (var source in sources)
                        {
                            _logger.ZLogInformation($"source: {source.Name} -> {source.Source} {source.Description}");
                        }
                        #endregion


                        #region metadata
                        PackageMetadataResource packageMetadataResource =
                            await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
                        IEnumerable<IPackageSearchMetadata> packages = await packageMetadataResource.GetMetadataAsync(
                            PACKAGE,
                            includePrerelease: false,
                            includeUnlisted: false,
                            cache,
                            new NullLogger(),
                            cancellationToken
                        );

                        foreach (IPackageSearchMetadata package in packages)
                        {
                            _logger.ZLogInformation($"Version: {package.Identity.Version}");
                            _logger.ZLogInformation($"Listed: {package.IsListed}");
                            _logger.ZLogInformation($"Tags: {package.Tags}");
                            _logger.ZLogInformation($"Description: {package.Description}");
                        }
                        #endregion

                        #region versions
                        FindPackageByIdResource packageIdResource =
                            await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
                        IEnumerable<NuGetVersion> versions = await packageIdResource.GetAllVersionsAsync(
                            PACKAGE,
                            cache,
                            new NullLogger(),
                            cancellationToken
                        );

                        foreach (NuGetVersion version in versions)
                        {
                            _logger.ZLogInformation($"Found version {version}");
                        }
                        #endregion

                        #region search packages
                        PackageSearchResource packageSearchResource =
                            await repository.GetResourceAsync<PackageSearchResource>(cancellationToken);
                        SearchFilter searchFilter = new SearchFilter(includePrerelease: true);

                        IEnumerable<IPackageSearchMetadata> results = await packageSearchResource.SearchAsync(
                            "json",
                            searchFilter,
                            skip: 0,
                            take: 20,
                            new NullLogger(),
                            cancellationToken
                        );

                        foreach (IPackageSearchMetadata result in results)
                        {
                            _logger.ZLogInformation($"Found package {result.Identity.Id} {result.Identity.Version}");
                        }
                        #endregion

                        #region download package
                        FindPackageByIdResource findPackageByIdResource =
                            await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

                        NuGetVersion packageVersion = new NuGetVersion("7.0.3");
                        using MemoryStream packageStream = new MemoryStream();

                        await findPackageByIdResource.CopyNupkgToStreamAsync(
                            PACKAGE,
                            packageVersion,
                            packageStream,
                            cache,
                            new NullLogger(),
                            cancellationToken
                        );

                        _logger.ZLogInformation($"Downloaded package {PACKAGE} {packageVersion}");

                        using PackageArchiveReader packageReader = new PackageArchiveReader(packageStream);

                        _logger.ZLogInformation($"Tags: {packageReader.NuspecReader.GetTags()}");
                        _logger.ZLogInformation($"Description: {packageReader.NuspecReader.GetDescription()}");
                        #endregion


                        #region read package
                        _logger.ZLogInformation($"ID: {packageReader.NuspecReader.GetId()}");

                        _logger.ZLogInformation($"Version: {packageReader.NuspecReader.GetVersion()}");
                        _logger.ZLogInformation($"Description: {packageReader.NuspecReader.GetDescription()}");
                        _logger.ZLogInformation($"Authors: {packageReader.NuspecReader.GetAuthors()}");

                        _logger.LogInformation("Dependencies:");
                        foreach (var dependencyGroup in packageReader.NuspecReader.GetDependencyGroups())
                        {
                            _logger.ZLogInformation($" - {dependencyGroup.TargetFramework.GetShortFolderName()}");
                            foreach (var dependency in dependencyGroup.Packages)
                            {
                                _logger.ZLogInformation($"   > {dependency.Id} {dependency.VersionRange}");
                            }
                        }

                        _logger.LogInformation("Files:");
                        foreach (var file in packageReader.GetFiles())
                        {
                            _logger.ZLogInformation($" - {file}");
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception!");
                    }
                    finally
                    {
                        // Stop the application once the work is done
                        _appLifetime.StopApplication();
                    }
                })
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose() { }
}
