using System;
using System.IO;
using System.CommandLine;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGettier.Core;

#nullable enable

public partial class Context : IDisposable
{
    public Uri source { get; protected set; }
    public SourceCacheContext cache { get; protected set; }
    public SourceRepository repository { get; protected set; }
    public IConsole console { get; set; }

    public Context(Uri source, IConsole console)
    {
        this.source = source;
        this.cache = new();
        this.repository = Repository.Factory.GetCoreV3($"{source.ToString()}");
        this.console = console;
    }

    public void Dispose() { }
}
