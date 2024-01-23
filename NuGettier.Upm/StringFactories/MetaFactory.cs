using System;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

public interface IMetaFactory
{
    string GenerateFolderMeta(string seed, string dirname);
    string GenerateFolderMeta(ulong seedHash, string dirname);
    string GenerateFileMeta(string seed, string filename);
    string GenerateFileMeta(ulong seedHash, string filename);
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
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        var guid = new MetaGen.Guid(Guid.SeedHash(seed), dirname);
        var metaTemplate = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.folder.meta")
        );
        var metaContents = metaTemplate(new { guid = guid });
        Logger.LogDebug(
            "generated meta file for folder {0} with seed {1} (GUID: {2}):\n{3}",
            dirname,
            seed,
            guid,
            metaContents
        );

        return metaContents;
    }

    public virtual string GenerateFolderMeta(ulong seedHash, string dirname)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        var guid = new MetaGen.Guid(seedHash, dirname);
        var metaTemplate = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.folder.meta")
        );
        var metaContents = metaTemplate(new { guid = guid });
        Logger.LogDebug(
            "generated meta file for folder {0} with seed hash {1} (GUID: {2}):\n{3}",
            dirname,
            seedHash,
            guid,
            metaContents
        );

        return metaContents;
    }

    public virtual string GenerateFileMeta(string seed, string filename)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        var guid = new MetaGen.Guid(Guid.SeedHash(seed), filename);
        var metaTemplate = Handlebars.Compile(
            Path.GetExtension(filename).EndsWith(".dll")
                ? EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.assembly.meta")
                : EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.template.meta")
        );
        var metaContents = metaTemplate(new { guid = guid });
        Logger.LogDebug(
            "generated meta file for file {0} with seed {1} (GUID: {2}):\n{3}",
            filename,
            seed,
            guid,
            metaContents
        );

        return metaContents;
    }
}
