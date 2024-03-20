using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Text.Json;
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
using NuGettier.Core;
using Xunit;

namespace NuGettier;

public partial class NuGettierService
{
    private Command ListDependenciesCommand =>
        new Command("deps", "retrieve information about a specific version of a given package")
        {
            PackageIdVersionArgument,
            IncludePrereleaseOption,
            OutputJsonOption,
            ShortOutputOption,
            SourceRepositoriesOption,
        }.WithHandler(CommandHandler.Create(ListDependencies));

    private async Task<int> ListDependencies(
        string packageIdVersion,
        bool preRelease,
        bool json,
        bool @short,
        IEnumerable<Uri> sources,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(nameof(ListDependencies));
        Assert.NotNull(Configuration);
        using var context = new Core.Context(
            host: Host,
            configuration: Configuration,
            loggerFactory: MainLoggerFactory,
            logger: MainLoggerFactory.CreateLogger<Context>(),
            console: console,
            sources: sources
        );
        var packages = await context.GetPackageDependencies(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );

        if (packages is null)
        {
            Logger.LogError("failed to get package dependencies for {0}", packageIdVersion);
            return 1;
        }

        if (@short)
        {
            var deps = packages.ToDictionary(p => p.Identity.Id, p => p.Identity.Version.ToNormalizedString());
            Assert.NotNull(deps);
            if (json)
            {
                Console.WriteLine(@$"{JsonSerializer.Serialize(deps, JsonOptions)}");
            }
            else
            {
                foreach (var kvp in deps)
                {
                    Console.WriteLine($"{kvp.Key}@{kvp.Value}");
                }
            }
            Logger.LogTrace("exit {0} without error (short mode)", nameof(ListDependencies));
            return 0;
        }

        if (json)
        {
            Console.WriteLine(@$"{JsonSerializer.Serialize(packages, JsonOptions)}");
        }
        else
        {
            foreach (var package in packages)
            {
                Console.WriteLine($"Identity: {package.Identity}");
                Console.WriteLine($"Version: {package.Identity.Version}");
                Console.WriteLine($"Listed: {package.IsListed}");
                Console.WriteLine($"Title: {package.Title}");
                Console.WriteLine($"Summary: {package.Summary}");
                Console.WriteLine($"Description: {package.Description}");
                Console.WriteLine($"Authors: {package.Authors}");
                Console.WriteLine($"Owners: {package.Owners}");
                Console.WriteLine($"Tags: {package.Tags}");
                Console.WriteLine($"License: {package.LicenseMetadata?.License}");
                Console.WriteLine($"Published: {package.Published}");
                Console.WriteLine($"Prefix Reserved: {package.PrefixReserved}");
                Console.WriteLine($"Project Url: {package.ProjectUrl}");
                Console.WriteLine($"License Url: {package.LicenseUrl}");
                Console.WriteLine($"Require License Acceptance: {package.RequireLicenseAcceptance}");
                Console.WriteLine($"Icon Url: {package.IconUrl}");
                Console.WriteLine($"Readme Url: {package.ReadmeUrl}");
                Console.WriteLine($"Report Abuse Url: {package.ReportAbuseUrl}");
                Console.WriteLine($"Package Details Url: {package.PackageDetailsUrl}");
                Console.WriteLine($"Dependencies:");
                foreach (PackageDependencyGroup dependencyGroup in package.DependencySets)
                {
                    Console.WriteLine($"- {dependencyGroup.TargetFramework}:");
                    // TODO: apply filter here
                    foreach (PackageDependency dependency in dependencyGroup.Packages)
                    {
                        Console.WriteLine($"  - {dependency}");
                    }
                }
            }
        }

        Logger.LogTrace("exit {0} without error", nameof(ListDependencies));
        return 0;
    }
}
