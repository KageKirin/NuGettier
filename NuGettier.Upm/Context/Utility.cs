using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.Text.RegularExpressions;
using NuGet.Protocol.Core.Types;
using Microsoft.Extensions.Configuration;
using HandlebarsDotNet;
using Xunit;

namespace NuGettier.Upm;

#nullable enable

public partial class Context
{
    public PackageRule GetPackageRule(string packageId)
    {
        var defaultRule = PackageRules.Where(r => string.IsNullOrEmpty(r.Id)).FirstOrDefault(DefaultPackageRule);
        var packageRule = PackageRules.Where(r => r.Id == packageId).FirstOrDefault(defaultRule);

        if (string.IsNullOrEmpty(packageRule.Name))
        {
            return new PackageRule(
                Id: packageRule.Id,
                Name: defaultRule.Name,
                Version: packageRule.Version,
                Framework: packageRule.Framework,
                IsIgnored: packageRule.IsIgnored,
                IsExcluded: packageRule.IsExcluded,
                IsRecursive: packageRule.IsRecursive
            );
        }

        return packageRule;
    }

    public virtual PackageJson PatchPackageJson(PackageJson packageJson)
    {
        // get packageRule for this package
        var packageRule = GetPackageRule(packageJson.Name);

        // patch packageJson.Name, .Version and .MinUnityVersion
        packageJson.Name = PatchPackageId(packageJson.Name);
        packageJson.Version = PatchPackageVersion(packageJson.Name, packageJson.Version);
        packageJson.MinUnityVersion = MinUnityVersion;

        // filter and patch dependencies' name and version
        packageJson.Dependencies = PatchPackageDependencies(packageJson.Dependencies);

        // basically tag as 'nugettier was here'
        packageJson.DevDependencies ??= new StringStringDictionary();
        packageJson.DevDependencies.Add(Build.AssemblyName, Build.AssemblyVersion);

        // add target
        packageJson.PublishingConfiguration ??= new PublishingConfiguration();
        packageJson.PublishingConfiguration.Registry = Target.ToString();

        // add repository/directory
        if (!string.IsNullOrEmpty(Repository))
            packageJson.Repository.Url = Repository;
        if (!string.IsNullOrEmpty(Directory))
            packageJson.Repository.Directory = Directory;

        return packageJson;
    }

    protected virtual string PatchPackageId(string packageId)
    {
        Console.WriteLine($"before: {packageId}");
        var metadata = CachedMetadata[packageId.ToLowerInvariant()];
        Assert.NotNull(metadata);

        var packageRule = GetPackageRule(packageId);
        string namingTemplate = !string.IsNullOrEmpty(packageRule.Name)
            ? packageRule.Name
            : Context.DefaultPackageRule.Name;

        var template = Handlebars.Compile(namingTemplate);
        var result = Regex.Replace(template(metadata).ToLowerInvariant().Replace(@" ", ""), @"\W", @".");
        Console.WriteLine($"after: {result}");
        return result;
    }

    protected virtual string PatchPackageVersion(string packageId, string packageVersion)
    {
        Console.WriteLine($"before: {packageId}: {packageVersion}");
        var packageRule = GetPackageRule(packageId);
        var versionRegex = !string.IsNullOrEmpty(packageRule.Version)
            ? packageRule.Version
            : Context.DefaultPackageRule.Version;

        var result = Regex.Match(packageVersion, versionRegex).Value;
        Console.WriteLine($"after: {packageId}: {result}");

        return result;
    }

    protected virtual IDictionary<string, string> PatchPackageDependencies(IDictionary<string, string> dependencies)
    {
        return dependencies
            .Where(d => GetPackageRule(d.Key).IsIgnored == false) //< filter: remove 'ignored' dependencies
            .ToDictionary(d => PatchPackageId(d.Key), d => PatchPackageVersion(d.Key, d.Value));
    }

    protected virtual PackageJson ConvertToPackageJson(IPackageSearchMetadata packageSearchMetadata)
    {
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
