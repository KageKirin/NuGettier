using System;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Tar;

namespace NuGettier.Upm.TarGz;

public sealed class TarDictionary : Dictionary<string, byte[]>, IDisposable
{
    public void Dispose()
    {
        Clear();
    }
}
