using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGettier.Upm.TarGz;

public static class FileDictionaryMetaExtension
{
    public static void AddMetaFiles(this FileDictionary fileDictionary, string seed, IMetaFactory metaFactory)
    {
        var folderEntries = fileDictionary
            .Keys.Where(f => Path.GetExtension(f) != @".meta")
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
                .Select(f => new KeyValuePair<string, string>($"{f}.meta", metaFactory.GenerateFolderMeta(seed, f)))
        );
        fileDictionary.AddRange(
            fileDictionary
                .Keys.Where(f => Path.GetExtension(f) != @".meta")
                .OrderBy(f => f.Length)
                .Select(f => new KeyValuePair<string, string>($"{f}.meta", metaFactory.GenerateFileMeta(seed, f)))
        );
    }
}
