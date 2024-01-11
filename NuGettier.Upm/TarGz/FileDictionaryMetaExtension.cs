using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGettier.Upm.TarGz;

public static class FileDictionaryMetaExtension
{
    public static void AddMetaFiles(this FileDictionary fileDictionary, string seed)
    {
        var folderEntries = fileDictionary
            .Keys
            .Where(file => Path.GetExtension(file) != @".meta")
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
            .ToHashSet();

        fileDictionary.AddRange(
            folderEntries
                .OrderBy(f => f.Length)
                .Select(file => new KeyValuePair<string, string>($"{file}.meta", Upm.MetaGen.GenerateMeta(seed, file, true)))
        );
        fileDictionary.AddRange(
            fileDictionary.Keys
                .Where(file => Path.GetExtension(file) != @".meta")
                .OrderBy(f => f.Length)
                .Select(file => new KeyValuePair<string, string>($"{file}.meta", Upm.MetaGen.GenerateMeta(seed, file, false)))
        );
    }
}
