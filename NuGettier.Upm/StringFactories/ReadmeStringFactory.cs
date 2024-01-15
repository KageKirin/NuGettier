using System;
using System.Reflection;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

public interface IReadmeStringFactory
{
    string GenerateReadme(
        string name,
        string version,
        string description,
        string applicationName,
        string applicationVersion
    );
}

public class ReadmeStringFactory : IReadmeStringFactory
{
    protected readonly Microsoft.Extensions.Logging.ILogger Logger;

    public ReadmeStringFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<ReadmeStringFactory>();
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
        return template(
            new
            {
                Name = name,
                Version = version,
                Description = description,
                ApplicationName = applicationName,
                ApplicationVersion = applicationVersion,
            }
        );
    }
}
