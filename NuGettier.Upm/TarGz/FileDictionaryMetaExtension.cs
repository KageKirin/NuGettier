using System;
using System.Linq;
using System.Collections.Generic;

namespace NuGettier.Upm.TarGz;

public static class FileDictionaryMetaExtension
{
    public static void AddMetaFiles(this FileDictionary fileDictionary, string seed)
    {
        fileDictionary.AddRange(
            fileDictionary.Keys
                .ToList() //< make a temp copy to avoid modifying the collection
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
