using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace NuGettier;

public static partial class Program
{
    private static Command UpmCommand =>
        new Command("upm", "root command for a number of commands specific to Unity packages")
        {
            UpmPackCommand,
            UpmUnpackCommand,
        };
}
