using System;
using System.Linq;
using System.Collections.Generic;

namespace NuGettier.Upm.TarGz;

public static class FileDictionaryMetaExtension
{
    public static void AddMetaFiles(this FileDictionary fileDictionary, string seed)
    {
        var files = fileDictionary.Keys.ToList(); //< make a temp copy to avoid modifying the collection

        // gather folders: Unity requires .meta files for each included folder as well
        var folders = files
            .SelectMany(f =>
            {
                var parents = new List<string>();
                var p = Path.GetDirectoryName(f);
                while (!string.IsNullOrEmpty(p))
                {
                    parents.Add(p);
                    p = Path.GetDirectoryName(p);
                }
                return parents;
            })
            .Distinct();

        fileDictionary.AddRange(
            files
                .Concat(folders) //< files AND folders
                .Where(file => Path.GetExtension(file) != @".meta")
                .Select(
                    file =>
                        new KeyValuePair<string, string>(
                            $"{file}.meta",
                            Upm.MetaGen.GenerateMeta(seed, file)
                        )
                )
        );
    }
}
