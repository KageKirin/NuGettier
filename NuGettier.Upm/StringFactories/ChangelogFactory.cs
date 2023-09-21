using System;
using System.Reflection;
using HandlebarsDotNet;

namespace NuGettier.Upm;

public static class ChangelogStringFactory
{
    public static string GenerateChangelog(string name, string version, string releaseNotes)
    {
        var template = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.CHANGELOG.md")
        );
        return template(
            new
            {
                Name = name,
                Version = version,
                ReleaseNotes = releaseNotes,
            }
        );
    }
}
