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
    private static Option<bool> OutputJsonOption =
        new(
            aliases: new string[] { "--json", "-j" },
            description: "whether to output result as JSON (for piping into `jq` etc)"
        );

    private static Option<bool> IncludePrereleaseOption =
        new(
            aliases: new string[] { "--preRelease", "-p" },
            description: "whether to include prerelease versions"
        );

    private static Option<bool> IncludeDependenciesOption =
        new(
            aliases: new string[] { "--includeDependencies", "-i" },
            description: "whether to include dependencies"
        );

    private static Option<bool> RetrieveLatestOption =
        new(aliases: new string[] { "--latest", "-l" }, description: "fetch the latest version");

    private static Option<string> SpecificVersionOption =
        new(aliases: new string[] { "--version", "-v" }, description: "version to fetch");

    private static Option<string> FrameworkOption =
        new(
            aliases: new string[] { "--framework", "-f" },
            description: "framework of DLL to repack"
        );

    private static Option<DirectoryInfo> OutputDirectoryOption =
        new(
            aliases: new string[] { "--outputDirectory", "-o" },
            description: "directory to output files to"
        )
        {
            IsRequired = true,
        };

    private static Option<Uri> SourceRepositoryOption =
        new(
            aliases: new string[] { "--source", "-s" },
            description: "source NuGet repository to fetch from"
        )
        {
            IsRequired = true,
        };

    private static Option<Uri> TargetRegistryOption =
        new(
            aliases: new string[] { "--target", "-t" },
            description: "target NPM registry to publish to"
        )
        {
            IsRequired = true,
        };

    private static void ValidateLatestOrVersion(CommandResult commandResult)
    {
        commandResult.ValidateOnlyOneOf(RetrieveLatestOption, SpecificVersionOption);
    }
}
