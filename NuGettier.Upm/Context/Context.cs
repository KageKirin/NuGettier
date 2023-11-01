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
    public record class PackageRule(string Id, bool IsIgnored, string Name, string Version, string Framework);

    public Uri Target { get; protected set; }
    public IEnumerable<PackageRule> PackageRules { get; protected set; }
    public IDictionary<string, IPackageSearchMetadata> CachedMetadata { get; protected set; }
    public string? Repository { get; protected set; }

    public Context(
        IConfigurationRoot configuration,
        IEnumerable<Uri> sources,
        Uri target,
        string? repository,
        IConsole console
    )
        : base(configuration, sources, console)
    {
        this.Target = target;
        this.Repository = repository;
        this.CachedMetadata = new Dictionary<string, IPackageSearchMetadata>();

        this.PackageRules = Configuration
            .GetSection(@"package")
            .GetChildren()
            .Select(packageSection =>
            {
                console.WriteLine($"package key: {packageSection.Key}");
                return new PackageRule(
                    Id: packageSection.Key,
                    IsIgnored: packageSection.GetValue<bool>("ignore"),
                    Name: packageSection.GetValue<string>("name") ?? string.Empty,
                    Version: packageSection.GetValue<string>("version") ?? string.Empty,
                    Framework: packageSection.GetValue<string>("framework") ?? string.Empty
                );
            })
            .Distinct();

        foreach (var p in PackageRules)
        {
            console.WriteLine($"{p}");
        }
    }

    public Context(Context other)
        : base(other)
    {
        Target = other.Target;
        Repository = other.Repository;
        PackageRules = other.PackageRules;
        CachedMetadata = other.CachedMetadata;
    }
}
