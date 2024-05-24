using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Core.Types;
using Xunit;

namespace NuGettier.Upm;

#nullable enable

public partial class Context
{
    public PackageRule GetPackageRule(string packageId)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        var defaultRule = PackageRules.Where(r => string.IsNullOrEmpty(r.Id)).FirstOrDefault(DefaultPackageRule);

        // descending order used for regex match, so that `.*` will match last
        // rationale: this allows to have generic rules for e.g. "Microsoft.Extensions.Logging.*" and "Microsoft.Extensions.*" that don't overlap
        var packageRulesByLengthDescending = PackageRules.OrderByDescending(r => r.Id.Length);

        // search for equality first, then regex to have e.g. a specific rule for "System.Text.Json" and a generic rule for "System.Text.*" that don't overlap
        var packageRule =
            packageRulesByLengthDescending
                .Where(r => r.Id == packageId)
                .FirstOrDefault()
            ?? packageRulesByLengthDescending
                .Where(r => Regex.IsMatch(r.Id, packageId))
                .FirstOrDefault()
            ?? packageRulesByLengthDescending
                .Where(r => Regex.IsMatch(r.Id, packageId, RegexOptions.IgnoreCase))
                .FirstOrDefault()
            ?? defaultRule;

        // create and return new package rule if retrieved one does not contain important information
        if (
            string.IsNullOrEmpty(packageRule.Name)
            || string.IsNullOrEmpty(packageRule.Version)
            || string.IsNullOrEmpty(packageRule.Framework)
        )
        {
            return new PackageRule(
                Id: packageRule.Id,
                Name: string.IsNullOrEmpty(packageRule.Name) ? defaultRule.Name : packageRule.Name,
                Version: string.IsNullOrEmpty(packageRule.Version) ? defaultRule.Version : packageRule.Version,
                Framework: string.IsNullOrEmpty(packageRule.Framework) ? defaultRule.Framework : packageRule.Framework,
                IsIgnored: packageRule.IsIgnored,
                IsExcluded: packageRule.IsExcluded,
                IsRecursive: packageRule.IsRecursive
            );
        }

        return packageRule;
    }

    public virtual PackageJson PatchPackageJson(
        PackageJson packageJson,
        string? prereleaseSuffix = null,
        string? buildmetaSuffix = null
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        // get packageRule for this package
        var packageRule = GetPackageRule(packageJson.Name);

        // patch packageJson.Name, .Version and .MinUnityVersion
        packageJson.Name = PatchPackageId(packageId: packageJson.Name);
        packageJson.Version = PatchPackageVersion(
            packageId: packageJson.Name,
            packageVersion: packageJson.Version,
            prereleaseSuffix: prereleaseSuffix,
            buildmetaSuffix: buildmetaSuffix
        );
        packageJson.MinUnityVersion = MinUnityVersion;

        // filter and patch dependencies' name and version
        packageJson.Dependencies = PatchPackageDependencies(packageJson.Dependencies);

        // basically tag as 'nugettier was here'
        packageJson.DevDependencies ??= new StringStringDictionary();
        packageJson.DevDependencies.Add(Build.AssemblyName, Build.AssemblyVersion);

        // add target
        packageJson.PublishingConfiguration ??= new PublishingConfiguration();
        packageJson.PublishingConfiguration.Registry = $"{Target.ToNpmFormat()}";

        // add repository/directory
        if (!string.IsNullOrEmpty(Repository))
            packageJson.Repository.Url = Repository;
        if (!string.IsNullOrEmpty(Directory))
            packageJson.Repository.Directory = Directory;

        return packageJson;
    }

    protected virtual string PatchPackageId(string packageId)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        Logger.LogTrace($"before: {packageId}");
        var metadata = CachedMetadata[packageId.ToLowerInvariant()];
        Assert.NotNull(metadata);

        var packageRule = GetPackageRule(packageId);
        string namingTemplate = !string.IsNullOrEmpty(packageRule.Name)
            ? packageRule.Name
            : Context.DefaultPackageRule.Name;

        var template = Handlebars.Compile(namingTemplate);
        var result = Regex.Replace(template(metadata).ToLowerInvariant().Replace(@" ", ""), @"\W", @".");
        Logger.LogTrace($"after: {result}");
        return result;
    }

    protected virtual string PatchPackageVersion(
        string packageId,
        string packageVersion,
        string? prereleaseSuffix = null,
        string? buildmetaSuffix = null
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        Logger.LogTrace($"before: {packageId}: {packageVersion}");
        var packageRule = GetPackageRule(packageId);
        var versionRegex = !string.IsNullOrEmpty(packageRule.Version)
            ? packageRule.Version
            : Context.DefaultPackageRule.Version;

        var resultVersion = Regex.Match(packageVersion, versionRegex).Value;
        Logger.LogTrace($"after: {packageId}: {resultVersion}");

        // add version suffixes
        if (!string.IsNullOrEmpty(prereleaseSuffix))
        {
            resultVersion += $"-{prereleaseSuffix}";
            Logger.LogDebug("adding: -{0} -> {1}", prereleaseSuffix, resultVersion);
        }

        if (!string.IsNullOrEmpty(buildmetaSuffix))
        {
            resultVersion += $"+{buildmetaSuffix}";
            Logger.LogDebug("adding: +{0} -> {1}", buildmetaSuffix, resultVersion);
        }

        return resultVersion;
    }

    protected virtual IDictionary<string, string> PatchPackageDependencies(IDictionary<string, string> dependencies)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        return dependencies
            .Where(d => GetPackageRule(d.Key).IsIgnored == false) //< filter: remove 'ignored' dependencies
            .ToDictionary(d => PatchPackageId(d.Key), d => PatchPackageVersion(d.Key, d.Value));
    }

    protected virtual PackageJson ConvertToPackageJson(IPackageSearchMetadata packageSearchMetadata)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        return new PackageJson()
        {
            Name = GetPackageId(packageSearchMetadata),
            Version = GetPackageVersion(packageSearchMetadata),
            License = GetPackageLicense(packageSearchMetadata) ?? string.Empty,
            Description = GetPackageDescription(packageSearchMetadata),
            DotNetFramework = NugetFramework.GetShortFolderName(),
            MinUnityVersion = MinUnityVersion,
            Homepage = GetPackageHomepage(packageSearchMetadata),
            Keywords = GetPackageKeywords(packageSearchMetadata),
            DisplayName = GetPackageDisplayName(packageSearchMetadata),
            Author = GetPackageAuthor(packageSearchMetadata),
            Contributors = GetPackageContributors(packageSearchMetadata),
            Repository = GetPackageRepository(packageSearchMetadata),
            PublishingConfiguration = GetPackagePublishingConfiguration(packageSearchMetadata),
            Dependencies = GetPackageDependencies(packageSearchMetadata, NugetFramework),
        };
    }
}
