using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NuGettier.Core;

#nullable enable

public partial class Context : IDisposable
{
    protected const string kSourceSection = @"source";
    protected const string kUsernameKey = @"username";
    protected const string kPasswordKey = @"password";
    protected const string kProtocolKey = @"protocol";
    protected const string kPackageSection = @"package";
    protected const string kIgnoreKey = @"ignore";
    protected const string kExcludeKey = @"exclude";
    protected const string kRecurseKey = @"recurse";
    protected const string kNameKey = @"name";
    protected const string kVersionKey = @"version";
    protected const string kFrameworkKey = @"framework";

    public record class BuildInfo(string AssemblyName, string AssemblyVersion);

    public record class PackageRule(
        string Id,
        string Name,
        string Version,
        string Framework,
        bool IsIgnored,
        bool IsExcluded,
        bool IsRecursive
    );

    public IHost Host { get; protected set; }
    public IConfigurationRoot Configuration { get; protected set; }
    public IEnumerable<Uri> Sources { get; protected set; }
    public SourceCacheContext Cache { get; protected set; }
    public IEnumerable<SourceRepository> Repositories { get; protected set; }
    public IConsole Console { get; protected set; }
    public BuildInfo Build { get; protected set; }
    public IEnumerable<PackageRule> PackageRules { get; protected set; }
    protected readonly ILoggerFactory LoggerFactory;
    protected readonly ILogger Logger;

    public Context(
        IHost host,
        IConfigurationRoot configuration,
        ILoggerFactory loggerFactory,
        ILogger logger,
        IConsole console,
        IEnumerable<Uri> sources
    )
    {
        Assert.NotNull(host);
        Host = host;

        Assert.NotNull(configuration);
        Configuration = configuration;

        Assert.NotNull(loggerFactory);
        LoggerFactory = loggerFactory;

        Assert.NotNull(logger);
        Logger = logger;

        Console = console;
        Cache = new();

        var entryAssembly = Assembly.GetEntryAssembly();
        var assemblyName = entryAssembly?.GetName();
        Logger.LogDebug("assembly name: {0}", assemblyName?.Name);
        Logger.LogDebug("assembly version: {0}", assemblyName?.Version?.ToString(3));
        Logger.LogDebug(
            "assembly informational version: {0}",
            entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        );

        Build = new(
            AssemblyName: assemblyName?.Name ?? @"unknown assembly",
            AssemblyVersion: entryAssembly
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
                ?? assemblyName?.Version?.ToString(3)
                ?? @"47.1.1"
        );

        Sources = Configuration
            .GetSection(kSourceSection)
            .GetChildren()
            .Select(
                sourceSection =>
                    new Uri(
                        (
                            string.IsNullOrEmpty(sourceSection.GetValue<string>(kUsernameKey))
                            && string.IsNullOrEmpty(sourceSection.GetValue<string>(kPasswordKey))
                        )
                            ? $"{sourceSection.GetValue<string>(kProtocolKey) ?? "https"}://{sourceSection.Key}"
                            : $"{sourceSection.GetValue<string>(kProtocolKey) ?? "https"}://{sourceSection[kUsernameKey]}:{sourceSection[kPasswordKey]}@{sourceSection.Key}"
                    )
            )
            .Concat(sources)
            .Distinct();

        Repositories = Sources.Select(source =>
        {
            var requestSource = source.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped);
            Logger.LogDebug("adding source {0}", requestSource);
            string? username = null;
            string? password = null;
            if (!string.IsNullOrEmpty(source.UserInfo))
            {
                var userInfo = source.UserInfo.Split(':');
                username = userInfo[0];
                password = userInfo[1];
                Logger.LogDebug("\tusername: {0}", username);
                Logger.LogDebug("\tpassword: {0}", password);
            }
            PackageSource packageSource =
                new(requestSource, source.Authority)
                {
                    Credentials =
                        (username is not null && password is not null)
                            ? PackageSourceCredential.FromUserInput(
                                source: source.Authority,
                                username: username,
                                password: password,
                                storePasswordInClearText: true,
                                validAuthenticationTypesText: null
                            )
                            : null,
                };
            return NuGet.Protocol.Core.Types.Repository.Factory.GetCoreV3(packageSource);
        });

        PackageRules = Configuration
            .GetSection(kPackageSection)
            .GetChildren()
            .Select(packageSection =>
            {
                Logger.LogDebug("creating package rule for key: {0}", packageSection.Key);
                return new PackageRule(
                    Id: packageSection.Key,
                    Name: packageSection.GetValue<string>(kNameKey) ?? string.Empty,
                    Version: packageSection.GetValue<string>(kVersionKey) ?? string.Empty,
                    Framework: packageSection.GetValue<string>(kFrameworkKey) ?? string.Empty,
                    IsIgnored: packageSection.GetValue<bool>(kIgnoreKey),
                    IsExcluded: packageSection.GetValue<bool>(kExcludeKey),
                    IsRecursive: packageSection.GetValue<bool>(kRecurseKey)
                );
            })
            .Distinct();

        foreach (var p in PackageRules)
        {
            Logger.LogDebug("package rule: {0}", p);
        }
    }

    public Context(Context other)
    {
        Host = other.Host;
        Configuration = other.Configuration;
        LoggerFactory = other.LoggerFactory;
        Logger = other.Logger;
        Console = other.Console;
        Sources = other.Sources;
        Cache = other.Cache;
        Build = other.Build;
        Repositories = other.Repositories;
        PackageRules = other.PackageRules;
    }

    public void Dispose() { }
}
