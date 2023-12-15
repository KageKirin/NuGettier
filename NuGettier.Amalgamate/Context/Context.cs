using System;
using System.Linq;
using System.Collections.Generic;
using System.CommandLine;
using NuGettier;
using NuGet.Protocol.Core.Types;
using Microsoft.Extensions.Configuration;

namespace NuGettier.Amalgamate;

public partial class Context : Upm.Context
{
    public Context(
        IConfigurationRoot configuration,
        IEnumerable<Uri> sources,
        string minUnityVersion,
        Uri target,
        string? repository,
        string? directory,
        IConsole console
    )
        : base(
            configuration: configuration,
            sources: sources,
            minUnityVersion: minUnityVersion,
            target: target,
            repository: repository,
            directory: directory,
            console: console
        ) { }

    public Context(Context other)
        : base(other) { }
}
