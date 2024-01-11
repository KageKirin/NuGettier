using System;
using HandlebarsDotNet;

namespace NuGettier.Upm;

public static partial class MetaGen
{
    public static string GenerateMeta(string seed, string filename)
    {
        var guid = new Guid(seed, filename);
        var metaTemplate = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.template.meta")
        );
        var metaContents = metaTemplate(new { guid = guid });

        return metaContents;
    }
}
