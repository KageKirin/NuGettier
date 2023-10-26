using System;
using System.CommandLine;
using NuGettier;

namespace NuGettier.Upm;

public partial class Context : Core.Context
{
    public Uri Target { get; protected set; }

    public Context(IEnumerable<Uri> sources, Uri target, IConsole console)
        : base(sources, console)
    {
        this.Target = target;
    }
}
