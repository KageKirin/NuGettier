using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Protocol.Core.Types;
using NuGettier;
using NuGettier.Core;

namespace NuGettier.Upm;

public partial class Context : Core.Context
{
    protected const string kUnitySection = @"unity";
    protected const string kDefaultFramework = @"netstandard2.0";

    public string MinUnityVersion { get; protected set; }
    public Uri Target { get; protected set; }

    public NuGetFramework NugetFramework { get; protected set; }
    public string Framework
    {
        get => NugetFramework.GetShortFolderName();
        [MemberNotNull(nameof(NugetFramework))]
        protected set { NugetFramework = NuGetFramework.Parse(value); }
    }

    public IDictionary<string, IPackageSearchMetadata> CachedMetadata { get; protected set; }
    public string? Repository { get; protected set; }
    public string? Directory { get; protected set; }

    public Context(
        IConfigurationRoot configuration,
        ILoggerFactory loggerFactory,
        ILogger logger,
        IEnumerable<Uri> sources,
        IConsole console,
        string minUnityVersion,
        Uri target,
        string? repository,
        string? directory
    )
        : base(
            configuration: configuration,
            loggerFactory: loggerFactory,
            logger: logger,
            console: console,
            sources: sources
        )
    {
        MinUnityVersion = minUnityVersion;
        Target = target;
        Repository = repository;
        Directory = directory;
        CachedMetadata = new Dictionary<string, IPackageSearchMetadata>();
        Framework = GetFrameworkFromUnitySettings(minUnityVersion);
    }

    public Context(Context other)
        : base(other)
    {
        MinUnityVersion = other.MinUnityVersion;
        Target = other.Target;
        Repository = other.Repository;
        Directory = other.Directory;
        CachedMetadata = other.CachedMetadata;
        NugetFramework = other.NugetFramework;
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
        if (!string.IsNullOrEmpty(framework))
            return framework;

        // 2nd choice: wildcard match from settings
        framework = unityToFramework
            .Where(kvp => kvp.Key.WildcardToRegex().IsMatch(minUnityVersion))
            .Select(kvp => kvp.Value)
            .FirstOrDefault();
        if (!string.IsNullOrEmpty(framework))
            return framework;

        // 3rd or last choice: default setting or hard-coded constant
        framework = Configuration.GetValue<string>(kFrameworkKey);
        if (!string.IsNullOrEmpty(framework))
            return framework;

        return kDefaultFramework;
    }
}
