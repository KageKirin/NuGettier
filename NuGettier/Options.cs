using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Completions;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NuGettier;

public partial class NuGettierService
{
    private Option<LogLevel> OutputLogLevelOption =
        new(aliases: ["--verbosity", "-v"], description: "log verbosity level");

    private Option<bool> OutputJsonOption =
        new(aliases: ["--json", "-j"], description: "whether to output result as JSON (for piping into `jq` etc)");

    private Option<bool> ShortOutputOption =
        new(aliases: ["--short", "-ß"], description: "whether to shorten output to the essential");

    private Option<bool> IncludePrereleaseOption =
        new(aliases: ["--preRelease", "-p"], description: "whether to include prerelease versions");

    private Option<bool> IncludeDependenciesOption =
        new(aliases: ["--includeDependencies", "-i"], description: "whether to include dependencies");

    private Option<DirectoryInfo> OutputDirectoryOption =
        new(
            aliases: ["--outputDirectory", "-o"],
            description: "directory to output files to",
            getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory)
        //new DirectoryInfo(
        //    Configuration.GetSection(kDefaultsSection).GetValue<string>(kOutputDirectoryKey)
        //        ?? Environment.CurrentDirectory
        //)
        )
        {
            IsRequired = true,
        };

    private Option<Uri[]> SourceRepositoriesOption =
        new(aliases: ["--source", "-s"], description: "source NuGet repositories to fetch from")
        {
            Name = "sources",
            IsRequired = false,
            AllowMultipleArgumentsPerToken = false, //< explicitly one value per --source argument
            Arity = ArgumentArity.OneOrMore,
        };

    private Option<Uri> TargetRegistryOption =
        new(
            aliases: ["--target", "-t"],
            description: "target NPM registry to publish to",
            getDefaultValue: () => new Uri("https://foo.bar")
        //new Uri(Configuration.GetSection(kDefaultsSection).GetValue<string>(kTargetKey) ?? "https://foo.bar")
        )
        {
            IsRequired = true,
        };

    private Option<bool> ForceOverwriteOption =
        new(aliases: ["--force", "-f"], description: "force overwrite existing files") { IsRequired = false, };
}
