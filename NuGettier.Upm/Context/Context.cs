using System;
using System.Linq;
using System.Collections.Generic;
using System.CommandLine;
using NuGettier;
using NuGet.Protocol.Core.Types;
using Microsoft.Extensions.Configuration;

namespace NuGettier.Upm;

public partial class Context : Core.Context
{
    protected const string kUnitySection = @"unity";
    protected const string kDefaultFramework = @"netstandard2.0";

    public string MinUnityVersion { get; protected set; }
    public Uri Target { get; protected set; }
    public IDictionary<string, string> SupportedFrameworks { get; protected set; }
    public IEnumerable<string> Frameworks
    {
        get => SupportedFrameworks.Keys.OrderDescending().ToArray();
    }
    public IDictionary<string, IPackageSearchMetadata> CachedMetadata { get; protected set; }
    public string? Repository { get; protected set; }
    public string? Directory { get; protected set; }

    public Context(
        IConfigurationRoot configuration,
        IEnumerable<Uri> sources,
        string minUnityVersion,
        Uri target,
        string? repository,
        string? directory,
        IConsole console
    )
        : base(configuration, sources, console)
    {
        this.MinUnityVersion = minUnityVersion;
        this.Target = target;
        this.Repository = repository;
        this.Directory = directory;
        this.CachedMetadata = new Dictionary<string, IPackageSearchMetadata>();

        this.SupportedFrameworks = new Dictionary<string, string>(DefaultSupportedFrameworks); //< cctor b/c modifications below
        foreach (var frameworkSection in Configuration.GetSection(kFrameworkSection).GetChildren())
        {
            var unityVersion = frameworkSection.GetValue<string>(kUnityKey);
            if (unityVersion != null)
            {
                console.WriteLine($"framework: {frameworkSection.Key} => {unityVersion}");
                SupportedFrameworks[frameworkSection.Key] = unityVersion;
            }

            var ignoreFlag = frameworkSection.GetValue<bool>(kIgnoreKey);
            if (ignoreFlag)
            {
                console.WriteLine($"deleting framework: {frameworkSection.Key}");
                SupportedFrameworks.Remove(frameworkSection.Key);
            }
        }
    }

    public Context(Context other)
        : base(other)
    {
        MinUnityVersion = other.MinUnityVersion;
        Target = other.Target;
        Repository = other.Repository;
        Directory = other.Directory;
        CachedMetadata = other.CachedMetadata;
        SupportedFrameworks = other.SupportedFrameworks;
    }

    internal string GetFrameworkFromUnitySettings(string minUnityVersion)
    {
        // load configuration
        var unityToFramework = Configuration
            .GetSection(kUnitySection)
            .GetChildren()
            .ToDictionary(
                unitySection => unitySection.Key,
                unitySection => unitySection.GetValue<string>(kFrameworkKey)
            );

        // 1st choice: direct match from settings
        var framework = unityToFramework
            .Where(kvp => kvp.Key == minUnityVersion)
            .Select(kvp => kvp.Value)
            .FirstOrDefault();
        Console.WriteLine($"framework (1st choice): {framework}");
        if (!string.IsNullOrEmpty(framework))
            return framework;

        // 2nd choice: wildcard match from settings
        framework = unityToFramework
            .Where(
                kvp =>
                    Regex.IsMatch(
                        minUnityVersion,
                        kvp.Key.Replace(@".", @"\.").Replace(@"%", @".?").Replace(@"*", @".*") //< wildcard to regex; TODO: string extension method
                    )
            )
            .Select(kvp => kvp.Value)
            .FirstOrDefault();
        Console.WriteLine($"framework (2nd choice): {framework}");
        if (!string.IsNullOrEmpty(framework))
            return framework;

        // 3rd or last choice: default setting or hard-coded constant
        framework = Configuration.GetValue<string>(kFrameworkKey);
        Console.WriteLine($"framework (3rd choice): {framework}");
        if (!string.IsNullOrEmpty(framework))
            return framework;

        return kDefaultFramework;
    }
}
