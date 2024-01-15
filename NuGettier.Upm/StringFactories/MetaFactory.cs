using System;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

public interface IMetaFactory
{
    string GenerateFolderMeta(string seed, string dirname);
    string GenerateFileMeta(string seed, string filename);
}

public class MetaFactory : IMetaFactory
{
    protected readonly Microsoft.Extensions.Logging.ILogger Logger;

    public MetaFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<MetaFactory>();
    }

    public virtual string GenerateFolderMeta(string seed, string dirname)
    {
        var guid = new MetaGen.Guid(seed, dirname);
        var metaTemplate = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.folder.meta")
        );
        var metaContents = metaTemplate(new { guid = guid });

        return metaContents;
    }

    public virtual string GenerateFileMeta(string seed, string filename)
    {
        var guid = new MetaGen.Guid(seed, filename);
        var metaTemplate = Handlebars.Compile(
            Path.GetExtension(filename).EndsWith(".dll")
                ? EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.assembly.meta")
                : EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.template.meta")
        );
        var metaContents = metaTemplate(new { guid = guid });

        return metaContents;
    }
}
