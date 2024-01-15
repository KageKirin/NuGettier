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

public class ReadmeFactory : IReadmeFactory
{
    protected readonly Microsoft.Extensions.Logging.ILogger Logger;

    public ReadmeFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<ReadmeFactory>();
    }

    public virtual string GenerateReadme(
        string name,
        string version,
        string description,
        string applicationName,
        string applicationVersion
    )
    {
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
}
