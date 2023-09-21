using System;
using System.Linq;
using System.Collections.Generic;

namespace NuGettier.Upm.TarGz;

public static class FileDictionaryMetaExtension
{
    public static void AddMetaFiles(this FileDictionary fileDictionary, string seed)
    {
        var fsEntries = fileDictionary.Keys
            .SelectMany(f =>
            // gather folders: Unity requires .meta files for each included folder as well
            {
                var parents = new HashSet<string>();
                var p = Path.GetDirectoryName(f);
                while (!string.IsNullOrEmpty(p))
                {
                    parents.Add(p);
                    p = Path.GetDirectoryName(p);
                }
                return parents;
            })
            .Distinct()
            .Concat(fileDictionary.Keys)
            .ToHashSet();

        fileDictionary.AddRange(
            fsEntries
                .Where(file => Path.GetExtension(file) != @".meta")
                .OrderBy(f => f.Length)
                .Select(file => new KeyValuePair<string, string>($"{file}.meta", Upm.MetaGen.GenerateMeta(seed, file)))
        );
    }
}
