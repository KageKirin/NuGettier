using System;
using System.Reflection;
using HandlebarsDotNet;

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
