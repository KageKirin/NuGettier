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
using Xunit;

namespace NuGettier;

public partial class NuGettierService
{
    private Command InfoCommand =>
        new Command("info", "retrieve information about a specific version of a given package")
        {
            PackageIdVersionArgument,
            IncludePrereleaseOption,
            OutputJsonOption,
            ShortOutputOption,
            SourceRepositoriesOption,
        }.WithHandler(CommandHandler.Create(Info));

    private async Task<int> Info(
        string packageIdVersion,
        bool preRelease,
        bool json,
        bool @short,
        IEnumerable<Uri> sources,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(nameof(Info));
        Assert.NotNull(Configuration);
        using var context = new Core.Context(
            configuration: Configuration!,
            sources: sources,
            console: console,
            loggerFactory: MainLoggerFactory
        );
        var package = await context.GetPackageInformation(
            packageIdVersion: packageIdVersion,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );

        if (package is null)
        {
            Logger.LogError("failed to get package information for {0}", packageIdVersion);
            return 1;
        }

        if (@short)
        {
            Dictionary<string, string> info =
                new()
                {
                    ["package"] = package.Identity.Id,
                    ["version"] = package.Identity.Version.ToNormalizedString(),
                    ["title"] = package.Title,
                    ["summary"] = package.Summary,
                    ["license"] = package.LicenseMetadata?.License ?? string.Empty,
                };
            if (json)
            {
                Console.WriteLine(@$"{JsonSerializer.Serialize(info, JsonOptions)}");
            }
            else
            {
                foreach (var kvp in info)
                {
                    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                }
            }
            Logger.LogTrace("exit {0} without error (short mode)", nameof(Info));
            return 0;
        }

        if (json)
        {
            Console.WriteLine(@$"{JsonSerializer.Serialize(package, JsonOptions)}");
        }
        else
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
                foreach (PackageDependency dependency in dependencyGroup.Packages)
                {
                    Console.WriteLine($"  - {dependency.ToString()}");
                }
            }

            //IEnumerable<PackageVulnerabilityMetadata> Vulnerabilities { get; }
            //Task<IEnumerable<VersionInfo>> GetVersionsAsync();
        }

        Logger.LogTrace("exit {0} without error", nameof(Info));
        return 0;
    }
}
