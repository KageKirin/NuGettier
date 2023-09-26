using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Completions;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;

namespace NuGettier;

public static partial class Program
{
    private static Option<string> UpmPrereleaseSuffixOption =
        new(
            aliases: new string[] { "--prerelease-suffix", },
            description: "version prerelease suffix ('foobar' -> '1.2.3-foobar+buildmeta)"
        );

    private static Option<string> UpmBuildmetaSuffixOption =
        new(
            aliases: new string[] { "--buildmeta-suffix", },
            description: "version buildmeta suffix ('foobar' -> '1.2.3-prerelease+foobar)"
        );
}
