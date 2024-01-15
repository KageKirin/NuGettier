using System;

namespace NuGettier.Upm;

#nullable enable

public partial record class PackageJson
{
    public virtual string GenerateReadme(string originalReadme, IReadmeFactory readmeFactory)
    {
        var nugettierDeps =
            this.DevDependencies?.FirstOrDefault() ?? new KeyValuePair<string, string>(@"unknown assembly", @"47.1.1");

        var readme = readmeFactory.GenerateReadme(
            name: $"{this.DisplayName} ({this.Name})",
            version: this.Version,
            description: this.Description,
            applicationName: nugettierDeps.Key,
            applicationVersion: nugettierDeps.Value
        );

        if (!string.IsNullOrEmpty(originalReadme))
        {
            readme += $"\n\n---\n\n{originalReadme}";
        }

        return readme;
    }

    public virtual string GenerateLicense(
        string originalLicense,
        string copyright,
        string copyrightHolder,
        ILicenseFactory licenseFactory
    )
    {
        var license = licenseFactory.GenerateLicense(
            name: $"{this.DisplayName} ({this.Name})",
            version: this.Version,
            copyright: copyright,
            copyrightHolder: copyrightHolder,
            license: this.License,
            licenseUrl: string.Empty
        );

        if (!string.IsNullOrEmpty(originalLicense))
        {
            license += $"\n\n---\n\n{originalLicense}";
        }

        return license;
    }

    public virtual string GenerateChangelog(string releaseNotes, IChangelogStringFactory changelogStringFactory)
    {
        return changelogStringFactory.GenerateChangelog(
            name: $"{this.DisplayName} ({this.Name})",
            version: this.Version,
            releaseNotes: releaseNotes
        );
    }
}
