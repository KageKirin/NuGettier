using System;
using HandlebarsDotNet;

namespace NuGettier.Upm;

public interface IMetaFactory
{
    string GenerateFolderMeta(string seed, string dirname);
    string GenerateFileMeta(string seed, string filename);
}

public static partial class MetaFactory
{
    public static string GenerateFolderMeta(string seed, string dirname)
    {
        var guid = new MetaGen.Guid(seed, dirname);
        var metaTemplate = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.folder.meta")
        );
        var metaContents = metaTemplate(new { guid = guid });

        return metaContents;
    }

    public static string GenerateFileMeta(string seed, string filename)
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
