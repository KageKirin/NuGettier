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
    public record class BuildInfo(string AssemblyName, string AssemblyVersion);

    public record class PackageRule(string Id, bool IsIgnored, string Name, string Version, string Framework);

    public IConfigurationRoot Configuration { get; protected set; }
    public IEnumerable<Uri> Sources { get; protected set; }
    public SourceCacheContext Cache { get; protected set; }
    public IEnumerable<SourceRepository> Repositories { get; protected set; }
    public IConsole Console { get; protected set; }
    public BuildInfo Build { get; protected set; }

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
            .GetSection(@"source")
            .GetChildren()
            .Select(
                sourceSection =>
                    new Uri(
                        (
                            string.IsNullOrEmpty(sourceSection.GetValue<string>("username"))
                            && string.IsNullOrEmpty(sourceSection.GetValue<string>("password"))
                        )
                            ? $"{sourceSection.GetValue<string>("protocol") ?? "https"}://{sourceSection.Key}"
                            : $"{sourceSection.GetValue<string>("protocol") ?? "https"}://{sourceSection["username"]}:{sourceSection["password"]}@{sourceSection.Key}"
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
    }

    public Context(Context other)
    {
        Configuration = other.Configuration;
        Console = other.Console;
        Sources = other.Sources;
        Cache = other.Cache;
        Build = other.Build;
        Repositories = other.Repositories;
    }

    public void Dispose() { }
}
