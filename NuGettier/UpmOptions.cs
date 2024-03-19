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

namespace NuGettier;

public partial class NuGettierService
{
    private Option<string> UpmUnityVersionOption =
        new(
            aliases: ["--unity", "-u"],
            description: "minimum Unity version required by package.json",
            getDefaultValue: () => "2022.3"
        //Configuration.GetSection(kDefaultsSection).GetValue<string>(kUnityKey) ?? "2022.3" //< latest LTS
        )
        {
            IsRequired = true,
        };

    private Option<string> UpmPrereleaseSuffixOption =
        new(
            aliases: ["--prerelease-suffix",],
            description: "version prerelease suffix ('foobar' -> '1.2.3-foobar+buildmeta)",
            getDefaultValue: () => string.Empty
        //Configuration.GetSection(kDefaultsSection).GetValue<string>(kPrereleaseSuffixKey) ?? string.Empty
        );

    private Option<string> UpmBuildmetaSuffixOption =
        new(
            aliases: ["--buildmeta-suffix",],
            description: "version buildmeta suffix ('foobar' -> '1.2.3-prerelease+foobar)",
            getDefaultValue: () => string.Empty
        //Configuration.GetSection(kDefaultsSection).GetValue<string>(kBuildmetaSuffixKey) ?? string.Empty
        );

    private Option<string> UpmTokenOption =
        new(aliases: ["--token",], description: "authentication token required to connect to NPM server")
        {
            IsRequired = false,
        };

    private Option<string> UpmNpmrcOption =
        new(aliases: ["--npmrc",], description: "path to existing .npmrc required to connect to NPM server")
        {
            IsRequired = false,
        };

    private Option<Uri> UpmRepositoryUrlOption =
        new(aliases: ["--repository",], description: "NPM package repository URL, assigned to `{.repository.url`}")
        {
            IsRequired = false,
        };

    private Option<string> UpmDirectoryUrlOption =
        new(aliases: ["--directory",], description: "NPM package directory path, assigned to `{.repository.directory`}")
        {
            IsRequired = false,
        };

    private Option<bool> UpmDryRunOption = new(aliases: ["--dry-run", "-n"], description: "Dry run");

    private Option<int> UpmTimeOutOption =
        new(
            aliases: ["--timeout", "-w"],
            description: "Time out (in seconds)",
            getDefaultValue: () => 60
        //Configuration.GetSection(kDefaultsSection).GetValue<int>(kTimeOutKey, 60)
        )
        {
            IsRequired = false,
        };

    private Option<Upm.PackageAccessLevel> UpmPackageAccessLevel =
        new(
            aliases: ["--access", "-a",],
            //getDefaultValue => Upm.PackageAccessLevel.Public,
            parseArgument: result =>
            {
                if (result.Tokens.Count != 1)
                {
                    result.ErrorMessage = "--access requires 1 argument";
                }

                return result.Tokens.First().Value.ToLowerInvariant() switch
                {
                    "public" => Upm.PackageAccessLevel.Public,
                    "private" => Upm.PackageAccessLevel.Private,
                    _ => Upm.PackageAccessLevel.Public
                };
            },
            description: $"package access level: [{string.Join("|", Enum.GetValues(typeof(Upm.PackageAccessLevel)).Cast<Upm.PackageAccessLevel>()).ToLowerInvariant()}]"
        );

    private Option<string> UpmMetagenSeedOption =
        new(aliases: ["--seed", "-s"], description: "seed string, e.g. package name") { IsRequired = false, };
}
