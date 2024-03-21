using System;
using System.Reflection;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

public interface IReadmeFactory
{
    string GenerateReadme(
        string name,
        string version,
        string description,
        string applicationName,
        string applicationVersion
    );
}

public class ReadmeFactory : IReadmeFactory, IDisposable
{
    protected readonly ILogger Logger;

    public ReadmeFactory(ILogger<ReadmeFactory> logger)
    {
        Logger = logger;
    }

    public virtual string GenerateReadme(
        string name,
        string version,
        string description,
        string applicationName,
        string applicationVersion
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        var template = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.README.md")
        );

        var generated = template(
            new
            {
                Name = name,
                Version = version,
                Description = description,
                ApplicationName = applicationName,
                ApplicationVersion = applicationVersion,
            }
        );
        Logger.LogDebug("generated readme:\n{0}", generated);
        return generated;
    }

    public virtual void Dispose() { }
}
