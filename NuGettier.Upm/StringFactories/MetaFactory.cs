using System;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

public interface IMetaFactory
{
    string GenerateFolderMeta(string dirname);
    string GenerateFileMeta(string filename);
}

public class MetaFactory : IMetaFactory, IDisposable
{
    protected readonly Microsoft.Extensions.Logging.ILogger Logger;
    protected string Seed;
    protected ulong SeedHash;

    public MetaFactory(string seed, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        Seed = seed;
        SeedHash = MetaGen.Guid.SeedHash(seed);
        Logger = loggerFactory.CreateLogger<MetaFactory>();
    }

    public virtual string GenerateFolderMeta(string dirname)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        var guid = new MetaGen.Guid(SeedHash, dirname);
        var metaTemplate = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.folder.meta")
        );
        var metaContents = metaTemplate(new { guid = guid });
        Logger.LogDebug(
            "generated meta file for folder {0} with seed {1} (hash {2:x}) (GUID: {3}):\n{4}",
            dirname,
            Seed,
            SeedHash,
            guid,
            metaContents
        );

        return metaContents;
    }

    public virtual string GenerateFileMeta(string filename)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        var guid = new MetaGen.Guid(SeedHash, filename);
        var metaTemplate = Handlebars.Compile(
            Path.GetExtension(filename).EndsWith(".dll")
                ? EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.assembly.meta")
                : EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.template.meta")
        );
        var metaContents = metaTemplate(new { guid = guid });
        Logger.LogDebug(
            "generated meta file for file {0} with seed {1} (hash {2:x}) (GUID: {3}):\n{4}",
            filename,
            Seed,
            SeedHash,
            guid,
            metaContents
        );

        return metaContents;
    }

    public virtual void Dispose() { }
}
