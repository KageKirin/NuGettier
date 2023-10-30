using System;

namespace NuGettier.Upm;

#nullable enable

public static class PackageJsonExtension
{
    public static string GenerateReadme(this PackageJson packageJson, string originalReadme)
    {
        var nugettierDeps =
            packageJson.DevDependencies?.FirstOrDefault()
            ?? new KeyValuePair<string, string>(@"unknown assembly", @"47.1.1");

        var readme = ReadmeStringFactory.GenerateReadme(
            name: $"{packageJson.DisplayName} ({packageJson.Name})",
            version: packageJson.Version,
            description: packageJson.Description,
            applicationName: nugettierDeps.Key,
            applicationVersion: nugettierDeps.Value
        );

        if (!string.IsNullOrEmpty(originalReadme))
        {
            readme += $"\n\n---\n\n{originalReadme}";
        }

        return readme;
    }
}
