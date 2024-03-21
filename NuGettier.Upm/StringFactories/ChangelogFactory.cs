using System;
using System.Reflection;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

public interface IChangelogFactory
{
    string GenerateChangelog(string name, string version, string releaseNotes);
}

public class ChangelogFactory : IChangelogFactory, IDisposable
{
    protected readonly ILogger Logger;

    public ChangelogFactory(ILogger<ChangelogFactory> logger)
    {
        Logger = logger;
    }

    public virtual string GenerateChangelog(string name, string version, string releaseNotes)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        var template = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.CHANGELOG.md")
        );

        var generated = template(
            new
            {
                Name = name,
                Version = version,
                ReleaseNotes = Uri.IsWellFormedUriString(releaseNotes, UriKind.Absolute)
                    ? $"[release notes]({releaseNotes})"
                    : releaseNotes,
            }
        );
        Logger.LogDebug("generated changelog:\n{0}", generated);
        return generated;
    }

    public virtual void Dispose() { }
}
