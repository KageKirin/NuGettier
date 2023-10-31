using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Packaging.Core;

namespace NuGettier.Upm;

public static class NuspecReaderExtension
{
    public static string GetUpmPackageName(
        this NuspecReader nuspecReader,
        IEnumerable<Context.PackageRule> packageRules
    )
    {
        return GetUpmPackageName(nuspecReader.GetAuthors(), nuspecReader.GetId(), packageRules);
    }

    public static string GetUpmPackageName(string author, string id, IEnumerable<Context.PackageRule> packageRules)
    {
        return packageRules.Where(p => p.Id == id && !string.IsNullOrEmpty(p.Name)).Select(p => p.Name).FirstOrDefault()
            ??
            // TODO: use config string + Handlebars template
            $"com.{author}.{id}".ToLowerInvariant().Replace(@" ", @"");
    }

    public static string GetUpmVersion(
        this NuspecReader nuspecReader,
        string? prereleaseSuffix = null,
        string? buildmetaSuffix = null
    )
    {
        var version = nuspecReader.GetVersion().ToString();
        if (prereleaseSuffix != null)
            version += $"-{prereleaseSuffix}";
        if (buildmetaSuffix != null)
            version += $"+{buildmetaSuffix}";

        return version;
    }

    public static string GetUpmName(this NuspecReader nuspecReader)
    {
        return string.IsNullOrWhiteSpace(nuspecReader.GetTitle()) ? nuspecReader.GetId() : nuspecReader.GetTitle();
    }

    public static string GetUpmDisplayName(this NuspecReader nuspecReader, string framework, AssemblyName assemblyName)
    {
        return nuspecReader.GetUpmName()
            + $" ({framework} DLL)"
            + $" [repacked by {assemblyName.Name} v{assemblyName.Version?.ToString()}]";
    }

    public static List<string> GetUpmKeywords(this NuspecReader nuspecReader)
    {
        List<string> keywords = new();
        if (!string.IsNullOrWhiteSpace(nuspecReader.GetTags()))
        {
            keywords.AddRange(nuspecReader.GetTags().Split(@" "));
        }
        return keywords;
    }

    public static StringStringDictionary GetUpmDependencies(
        this NuspecReader nuspecReader,
        string framework,
        Func<string, string, Task<string?>> getDependencyName,
        IEnumerable<Context.PackageRule> packageRules
    )
    {
        return new StringStringDictionary(
            nuspecReader
                .GetDependencyGroups()
                .Where(d => d.TargetFramework.GetShortFolderName() == framework)
                .SelectMany(d => d.Packages)
                .Select(
                    p =>
                        new KeyValuePair<PackageDependency, Context.PackageRule>(
                            p,
                            packageRules.Where(r => r.Id == p.Id).FirstOrDefault(Upm.Context.DefaultPackageRule)
                        )
                )
                .Where(pr => !pr.Value.IsIgnored)
                .ToDictionary(
                    pr => pr.Value.Name,
                    pr =>
                    {
                        if (!string.IsNullOrEmpty(pr.Value.Version))
                        {
                            string match = Regex
                                .Match(pr.Key.VersionRange.ToLegacyShortString(), pr.Value.Version)
                                .Value;
                            if (!string.IsNullOrEmpty(match))
                                return match;
                        }
                        return pr.Key.VersionRange.ToLegacyShortString();
                    }
                )
        );
    }

    public static Repository GetUpmRepository(this NuspecReader nuspecReader)
    {
        return new Repository() { Url = nuspecReader.GetProjectUrl(), };
    }

    public static PublishingConfiguration GetUpmPublishingConfiguration(
        this NuspecReader nuspecReader,
        Uri targetRegistry
    )
    {
        return new PublishingConfiguration() { Registry = targetRegistry.ToString(), };
    }

    public static PackageJson GenerateUpmPackageJson(
        this NuspecReader nuspecReader,
        string framework,
        Uri targetRegistry,
        AssemblyName assemblyName,
        Func<string, string, Task<string?>> getDependencyName,
        IEnumerable<Context.PackageRule> packageRules,
        string? prereleaseSuffix = null,
        string? buildmetaSuffix = null
    )
    {
        PackageJson packageJson =
            new()
            {
                Name = nuspecReader.GetUpmPackageName(packageRules),
                Version = nuspecReader.GetUpmVersion(prereleaseSuffix, buildmetaSuffix),
                License = nuspecReader.GetLicenseMetadata()?.License,
                Description = nuspecReader.GetDescription(),
                Keywords = nuspecReader.GetUpmKeywords(),
                DisplayName = nuspecReader.GetUpmDisplayName(framework, assemblyName),
                Repository = nuspecReader.GetUpmRepository(),
                PublishingConfiguration = nuspecReader.GetUpmPublishingConfiguration(targetRegistry),
                Dependencies = nuspecReader.GetUpmDependencies(framework, getDependencyName, packageRules),
            };

        return packageJson;
    }
}
