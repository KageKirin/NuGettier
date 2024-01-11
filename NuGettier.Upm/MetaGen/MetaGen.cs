using System;
using HandlebarsDotNet;

namespace NuGettier.Upm;

public static partial class MetaGen
{
    public static string GenerateFolderMeta(string seed, string dirname)
    {
        var guid = new Guid(seed, dirname);
        var metaTemplate = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.folder.meta")
        );
        var metaContents = metaTemplate(new { guid = guid });

        return metaContents;
    }

    public static string GenerateFileMeta(string seed, string filename)
    {
        var guid = new Guid(seed, filename);
        var metaTemplate = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.template.meta")
        );
        var metaContents = metaTemplate(new { guid = guid });

        return metaContents;
    }
}
