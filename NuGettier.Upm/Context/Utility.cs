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
    public PackageJson PatchPackageJson(PackageJson packageJson)
    {
        // patch packageJson.Name and .Version
        packageJson.Name = PatchPackageName(packageJson.Name);
        packageJson.Version = PatchPackageVersion(packageJson.Name, packageJson.Version);

        // filter and patch dependencies' name and version
        packageJson.Dependencies = packageJson.Dependencies
            .Where(d => !PackageRules.Any(r => r.Id == d.Key && r.IsIgnored == true)) //< filter
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

        var packageRule =
            PackageRules.Where(r => r.Id == packageName).FirstOrDefault()
            ?? PackageRules.Where(r => string.IsNullOrEmpty(r.Id)).FirstOrDefault();

        string namingTemplate = packageRule?.Name ?? Context.DefaultPackageRule.Name;

        var template = Handlebars.Compile(namingTemplate);
        var result = template(CachedMetadata[packageName]).ToLowerInvariant().Replace(@" ", @"");
        Console.WriteLine($"after: {result}");
        return result;
    }

    private string PatchPackageVersion(string packageName, string packageVersion)
    {
        Console.WriteLine($"before: {packageName}: {packageVersion}");
        var packageRule =
            PackageRules.Where(r => r.Id == packageName).FirstOrDefault()
            ?? PackageRules.Where(r => string.IsNullOrEmpty(r.Id)).FirstOrDefault();

        string? versionRegex = packageRule?.Version;
        if (string.IsNullOrEmpty(versionRegex))
            versionRegex = Context.DefaultPackageRule.Version;

        var result = Regex.Match(packageVersion, versionRegex).Value;
        Console.WriteLine($"after: {packageName}: {result}");

        return result;
    }
}
