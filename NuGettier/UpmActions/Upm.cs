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
    private Command UpmCommand =>
        new Command("upm", "root command for a number of commands specific to Unity packages")
        {
            UpmInfoCommand,
            UpmPackCommand,
            UpmUnpackCommand,
            UpmPublishCommand,
            UpmNpmrcCommand,
            UpmMetagenCommand,
            UpmPublishPackageCommand,
            AmalgamateCommand,
        };
}
