using System;
using HandlebarsDotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

public interface IMetaFactory
{
    void InitializeWithSeed(string seed);
    string GenerateFolderMeta(string dirname);
    string GenerateFileMeta(string filename);
}

public class MetaFactory : IMetaFactory, IDisposable
{
    protected readonly ILogger Logger;
    protected readonly IGuidFactory GuidFactory;

    public MetaFactory(ILogger<MetaFactory> logger, [FromKeyedServices("sha1")] IGuidFactory guidFactory)
    {
        Logger = logger;
        GuidFactory = guidFactory;
    }

    public virtual void InitializeWithSeed(string seed)
    {
        GuidFactory.InitializeWithSeed(seed);
    }

    public virtual string GenerateFolderMeta(string dirname)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        var uuid = GuidFactory.GenerateGuid(dirname).ToRfc4122().ToUnityString();
        var metaTemplate = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.folder.meta")
        );
        var metaContents = metaTemplate(new { guid = uuid });
        Logger.LogDebug("generated meta file for folder {0} with GUID: {1}:\n{2}", dirname, uuid, metaContents);

        return metaContents;
    }

    public virtual string GenerateFileMeta(string filename)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        var uuid = GuidFactory.GenerateGuid(filename).ToRfc4122().ToUnityString();
        var metaTemplate = Handlebars.Compile(
            Path.GetExtension(filename).EndsWith(".dll")
                ? EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.assembly.meta")
                : EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.template.meta")
        );
        var metaContents = metaTemplate(new { guid = uuid });
        Logger.LogDebug("generated meta file for file {0} with GUID: {1}:\n{2}", filename, uuid, metaContents);

        return metaContents;
    }

    public virtual void Dispose() { }
}
