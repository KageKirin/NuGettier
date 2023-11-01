using System;
using System.Linq;
using System.Collections.Generic;
using System.CommandLine;
using System.Text.RegularExpressions;
using NuGet.Protocol.Core.Types;
using Microsoft.Extensions.Configuration;
using HandlebarsDotNet;
using Xunit;

namespace NuGettier.Upm;

public partial class Context
{
    public PackageRule GetPackageRule(string packageId)
    {
        return PackageRules
            .Where(r => r.Id == packageId)
            .FirstOrDefault(PackageRules.Where(r => string.IsNullOrEmpty(r.Id)).FirstOrDefault(DefaultPackageRule));
    }

    public PackageJson PatchPackageJson(PackageJson packageJson)
    {
        // get packageRule for this package
        var packageRule = GetPackageRule(packageJson.Name);

        // patch packageJson.Name and .Version
        packageJson.Name = PatchPackageName(packageJson.Name);
        packageJson.Version = PatchPackageVersion(packageJson.Name, packageJson.Version);

        // filter and patch dependencies' name and version
        packageJson.Dependencies = packageJson.Dependencies
            .Where(d => GetPackageRule(d.Key).IsIgnored == false) //< filter
            .ToDictionary(d => PatchPackageName(d.Key), d => PatchPackageVersion(d.Key, d.Value));

        // basically tag as 'nugettier was here'
        packageJson.DevDependencies ??= new StringStringDictionary();
        packageJson.DevDependencies.Add(Build.AssemblyName, Build.AssemblyVersion);

        // add target
        packageJson.PublishingConfiguration ??= new PublishingConfiguration();
        packageJson.PublishingConfiguration.Registry = Target.ToString();

        return packageJson;
    }

    private string PatchPackageName(string packageName)
    {
        Console.WriteLine($"before: {packageName}");
        Assert.NotNull(CachedMetadata[packageName]);

        var packageRule = GetPackageRule(packageName);
        string namingTemplate = !string.IsNullOrEmpty(packageRule.Name) ? packageRule.Name : Context.DefaultPackageRule.Name;

        var template = Handlebars.Compile(namingTemplate);
        var result = template(CachedMetadata[packageName]).ToLowerInvariant().Replace(@" ", @"");
        Console.WriteLine($"after: {result}");
        return result;
    }

    private string PatchPackageVersion(string packageName, string packageVersion)
    {
        Console.WriteLine($"before: {packageName}: {packageVersion}");
        var packageRule = GetPackageRule(packageName);
        var versionRegex = !string.IsNullOrEmpty(packageRule.Version) ? packageRule.Version : Context.DefaultPackageRule.Version;

        var result = Regex.Match(packageVersion, versionRegex).Value;
        Console.WriteLine($"after: {packageName}: {result}");

        return result;
    }
}
