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

    private static Option<string> UpmTokenOption =
        new(aliases: new string[] { "--token", }, description: "authentication token required to connect to NPM server")
        {
            IsRequired = false,
        };

    private static Option<string> UpmNpmrcOption =
        new(
            aliases: new string[] { "--npmrc", },
            description: "path to existing .npmrc required to connect to NPM server"
        )
        {
            IsRequired = false,
        };

    private static Option<Uri> UpmRepositoryUrlOption =
        new(
            aliases: new string[] { "--repository", },
            description: "NPM package repository URL, assigned to `{.repository.url`}"
        )
        {
            IsRequired = false,
        };

    private static Option<string> UpmDirectoryUrlOption =
        new(
            aliases: new string[] { "--directory", },
            description: "NPM package directory path, assigned to `{.repository.directory`}"
        )
        {
            IsRequired = false,
        };

    private static Option<bool> UpmDryRunOption =
        new(aliases: new string[] { "--dry-run", "-n" }, description: "Dry run");

    private static Option<Upm.PackageAccessLevel> UpmPackageAccessLevel =
        new(
            aliases: new string[] { "--access", "-a", },
            //getDefaultValue => Upm.PackageAccessLevel.Public,
            parseArgument: result =>
            {
                if (result.Tokens.Count != 1)
                {
                    result.ErrorMessage = "--access requires 1 argument";
                }
                else
                {
                    switch (result.Tokens.First().Value.ToLowerInvariant())
                    {
                        case "public":
                            return Upm.PackageAccessLevel.Public;

                        case "private":
                            return Upm.PackageAccessLevel.Private;
                    }
                }
                return Upm.PackageAccessLevel.Public;
            },
            description: $"package access level: [{string.Join("|", Enum.GetValues(typeof(Upm.PackageAccessLevel)).Cast<Upm.PackageAccessLevel>()).ToLowerInvariant()}]"
        );
}
