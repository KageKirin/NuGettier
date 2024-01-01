using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NLog;
using NuGet.Protocol.Core.Types;
using NuGettier;

namespace NuGettier.Amalgamate;

public partial class Context : Upm.Context
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
