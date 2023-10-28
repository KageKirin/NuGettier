using System;
using System.CommandLine;
using NuGettier;
using Microsoft.Extensions.Configuration;

namespace NuGettier.Upm;

public partial class Context : Core.Context
{
    public record struct PackageRule(string Id, bool IsIgnored, string Name, string Version);

    public Uri Target { get; protected set; }

    public Context(IConfigurationRoot configuration, IEnumerable<Uri> sources, Uri target, IConsole console)
        : base(configuration, sources, console)
    {
        this.Target = target;
    }

    public Context(Context other)
        : base(other)
    {
        Target = other.Target;
    }
}
