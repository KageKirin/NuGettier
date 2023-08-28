using System;

namespace NuGettier.Upm;

public static partial class MetaGen
{
    public static string GenerateMeta(string seed, string filename)
    {
        var guid = new Guid(seed, filename);
        var metaContents =
            @$"fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";
        return metaContents;
    }
}
