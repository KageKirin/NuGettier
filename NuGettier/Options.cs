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

    private static Option<bool> ShortOutputOption =
        new(aliases: new string[] { "--short", "-ß" }, description: "whether to shorten output to the essential");

    private static Option<bool> IncludePrereleaseOption =
        new(aliases: new string[] { "--preRelease", "-p" }, description: "whether to include prerelease versions");

    private static Option<bool> IncludeDependenciesOption =
        new(aliases: new string[] { "--includeDependencies", "-i" }, description: "whether to include dependencies");

    private static Option<DirectoryInfo> OutputDirectoryOption =
        new(aliases: new string[] { "--outputDirectory", "-o" }, description: "directory to output files to")
        {
            IsRequired = true,
        };

    private static Option<Uri[]> SourceRepositoriesOption =
        new(aliases: new string[] { "--source", "-s" }, description: "source NuGet repositories to fetch from")
        {
            Name = "sources",
            IsRequired = false,
            AllowMultipleArgumentsPerToken = false, //< explicitly one value per --source argument
            Arity = ArgumentArity.OneOrMore,
        };

    [Obsolete("'--username' and '--password' options have been removed", true)]
    private static Option<string> SourceRepositoryUsernameOption =
        new(
            aliases: new string[] { "--username", },
            description: "username or API user for source NuGet repository to fetch from"
        )
        {
            IsRequired = false, // optional, b/c public repos don't need one
        };

    [Obsolete("'--username' and '--password' options have been removed", true)]
    private static Option<string> SourceRepositoryPasswordOption =
        new(
            aliases: new string[] { "--password", },
            description: "password or API key for source NuGet repository to fetch from"
        )
        {
            IsRequired = false, // optional, b/c public repos don't need one
        };

    private static Option<Uri> TargetRegistryOption =
        new(aliases: new string[] { "--target", "-t" }, description: "target NPM registry to publish to")
        {
            IsRequired = true,
        };

    [Obsolete("'--latest' and '--version' options have been removed", true)]
    private static void ValidateLatestOrVersion(CommandResult commandResult)
    {
        commandResult.ValidateOnlyOneOf(RetrieveLatestOption, SpecificVersionOption);
    }
}
