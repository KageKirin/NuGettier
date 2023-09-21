using System;
using System.CommandLine;
using NuGettier;

namespace NuGettier.Upm;

public partial class Context : Core.Context
{
    public Uri target { get; protected set; }

    public Context(Uri source, Uri target, IConsole console)
        : base(source, console)
    {
        this.target = target;
    }
}
