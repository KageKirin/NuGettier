using System;
using System.CommandLine;
using NuGettier;

namespace NuGettier.Upm;

public partial class Context : Core.Context
{
    public Context(Uri source, IConsole console)
        : base(source, console)
    {
    }
}
