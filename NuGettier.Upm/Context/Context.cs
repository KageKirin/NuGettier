using System;
using System.Linq;
using System.Collections.Generic;
using System.CommandLine;
using NuGettier;
using Microsoft.Extensions.Configuration;

namespace NuGettier.Upm;

public partial class Context : Core.Context
{
    public record struct PackageRule(string Id, bool IsIgnored, string Name, string Version);

    public Uri Target { get; protected set; }
    public IEnumerable<PackageRule> PackageRules { get; protected set; }

    public Context(IConfigurationRoot configuration, IEnumerable<Uri> sources, Uri target, IConsole console)
        : base(configuration, sources, console)
    {
        this.Target = target;

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
                    Version: packageSection.GetValue<string>("version") ?? string.Empty
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
        PackageRules = other.PackageRules;
    }
}
