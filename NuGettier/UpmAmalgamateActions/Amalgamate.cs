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
    private static Command AmalgamateCommand =>
        new Command("amalgamate", "root command for a number of commands specific to amalgamated Unity packages")
        {
            AmalgamateInfoCommand,
            AmalgamatePackCommand,
        };
}
