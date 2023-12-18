using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.CommandLine;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Microsoft.Extensions.Configuration;
using Xunit;

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

    public IConfigurationRoot Configuration { get; protected set; }
    public IEnumerable<Uri> Sources { get; protected set; }
    public SourceCacheContext Cache { get; protected set; }
    public IEnumerable<SourceRepository> Repositories { get; protected set; }
    public IConsole Console { get; protected set; }
    public BuildInfo Build { get; protected set; }
    public IEnumerable<PackageRule> PackageRules { get; protected set; }

    public Context(IConfigurationRoot configuration, IEnumerable<Uri> sources, IConsole console)
    {
        Assert.NotNull(configuration);
        Configuration = configuration;
        Console = console;
        Cache = new();

        var entryAssembly = Assembly.GetEntryAssembly();
        var assemblyName = entryAssembly?.GetName();
        console.WriteLine($"assembly name: {assemblyName?.Name}");
        console.WriteLine($"assembly version: {assemblyName?.Version?.ToString(3)}");
        console.WriteLine(
            $"assembly informational version: {entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}"
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
            console.WriteLine($"adding source {requestSource}");
            string? username = null;
            string? password = null;
            if (!string.IsNullOrEmpty(source.UserInfo))
            {
                var userInfo = source.UserInfo.Split(':');
                username = userInfo[0];
                password = userInfo[1];
                console.WriteLine($"\tusername: {username}");
                console.WriteLine($"\tpassword: {password}");
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
                console.WriteLine($"package key: {packageSection.Key}");
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
            console.WriteLine($"{p}");
        }
    }

    public Context(Context other)
    {
        Configuration = other.Configuration;
        Console = other.Console;
        Sources = other.Sources;
        Cache = other.Cache;
        Build = other.Build;
        Repositories = other.Repositories;
        PackageRules = other.PackageRules;
    }

    public void Dispose() { }
}
