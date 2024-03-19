using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGettier.Upm;
using NuGettier.Upm.TarGz;
using Xunit;

namespace NuGettier;

public partial class NuGettierService
{
    private Command AmalgamateInfoCommand =>
        new Command("info", "preview Unity package informations for the given NuPkg at the given version")
        {
            PackageIdVersionArgument,
            OutputJsonOption,
            IncludePrereleaseOption,
            SourceRepositoriesOption,
            TargetRegistryOption,
            UpmUnityVersionOption,
            UpmPrereleaseSuffixOption,
            UpmBuildmetaSuffixOption,
            UpmRepositoryUrlOption,
            UpmDirectoryUrlOption,
        }.WithHandler(CommandHandler.Create(AmalgamateInfo));

    private async Task<int> AmalgamateInfo(
        string packageIdVersion,
        bool json,
        bool preRelease,
        IEnumerable<Uri> sources,
        Uri target,
        string unity,
        string? prereleaseSuffix,
        string? buildmetaSuffix,
        string? repository,
        string? directory,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(nameof(AmalgamateInfo));
        Assert.NotNull(Configuration);
        using var context = new Amalgamate.Context(
            configuration: Configuration!,
            sources: sources,
            minUnityVersion: unity,
            target: target,
            repository: repository,
            directory: directory,
            console: console,
            loggerFactory: MainLoggerFactory
        );

        var packageJson = await context.GetPackageJson(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            prereleaseSuffix: prereleaseSuffix,
            buildmetaSuffix: buildmetaSuffix,
            cancellationToken: cancellationToken
        );

        if (packageJson == null)
        {
            Logger.LogError("failed to generate package.json for {0} amalgamated UPM package", packageIdVersion);
            return 1;
        }

        if (json)
        {
            console.WriteLine(@$"{packageJson.ToJson()}");
        }
        else
        {
            Console.WriteLine($"Name: {packageJson.Name}");
            Console.WriteLine($"Version: {packageJson.Version}");
            Console.WriteLine($"DisplayName: {packageJson.DisplayName}");
            Console.WriteLine($"Description: {packageJson.Description}");
            Console.WriteLine($"License: {packageJson.License}");
            Console.WriteLine(
                $"Author: {packageJson.Author.Name} <{packageJson.Author.Email}> ({packageJson.Author.Url})"
            );
            Console.WriteLine(
                $"Repository: {packageJson.Repository.Url} ({packageJson.Repository.RepoType}) {packageJson.Repository.Directory}"
            );
            Console.WriteLine($"Registry: {packageJson.PublishingConfiguration.Registry}");
            Console.WriteLine($"Keywords: {string.Join(", ", packageJson.Keywords)}");
            Console.WriteLine($"Files: {string.Join(", ", packageJson.Files)}");
            Console.WriteLine($"Dependencies:");
            foreach (var kvp in packageJson.Dependencies)
            {
                Console.WriteLine($"- {kvp.Key} @{kvp.Value}");
            }
        }

        Logger.LogTrace("exit {0} without error", nameof(AmalgamateInfo));
        return 0;
    }
}
