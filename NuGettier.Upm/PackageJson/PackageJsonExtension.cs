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

    public static string GenerateLicense(
        this PackageJson packageJson,
        string originalLicense,
        string copyright,
        string copyrightHolder
    )
    {
        var license = LicenseStringFactory.GenerateLicense(
            name: $"{packageJson.DisplayName} ({packageJson.Name})",
            version: packageJson.Version,
            copyright: copyright,
            copyrightHolder: copyrightHolder,
            license: packageJson.License,
            licenseUrl: string.Empty
        );

        if (!string.IsNullOrEmpty(originalLicense))
        {
            license += $"\n\n---\n\n{originalLicense}";
        }

        return license;
    }

    public static string GenerateChangelog(this PackageJson packageJson, string releaseNotes)
    {
        return ChangelogStringFactory.GenerateChangelog(
            name: $"{packageJson.DisplayName} ({packageJson.Name})",
            version: packageJson.Version,
            releaseNotes: releaseNotes
        );
    }
}
