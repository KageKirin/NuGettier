using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NuGettier;

public partial class NuGettierService
{
    private Command AmalgamateCommand =>
        new Command("amalgamate", "root command for a number of commands specific to amalgamated Unity packages")
        {
            AmalgamateInfoCommand,
            AmalgamatePackCommand,
            AmalgamateUnpackCommand,
            AmalgamatePublishCommand,
        };
}
