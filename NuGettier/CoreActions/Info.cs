using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGettier;

public static partial class Program
{
    private static Command InfoCommand =>
        new Command("info", "retrieve information about a specific version of a given package")
        {
            PackageNameArgument,
            IncludePrereleaseOption,
            RetrieveLatestOption,
            SpecificVersionOption,
            OutputJsonOption,
            SourceRepositoryOption,
            SourceRepositoryUsernameOption,
            SourceRepositoryPasswordOption,
        }
            .WithValidator(ValidateLatestOrVersion)
            .WithHandler(CommandHandler.Create(Info));

    private static async Task<int> Info(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        bool json,
        Uri source,
        string? username,
        string? password,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var context = new Core.Context(source: source, username: username, password: password, console: console);
        var package = await context.GetPackageInformation(
            packageName: packageName,
            preRelease: preRelease,
            latest: latest,
            version: version,
            cancellationToken: cancellationToken
        );

        if (package != null)
        {
            if (json)
            {
                Console.WriteLine($"{JsonSerializer.Serialize(package)}");
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
        }

        return 0;
    }
}
