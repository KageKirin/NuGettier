using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Core.Types;
using NuGettier;

namespace NuGettier.Amalgamate;

public partial class Context : Upm.Context
{
    public Context(
        IHost host,
        IConfigurationRoot configuration,
        ILoggerFactory loggerFactory,
        ILogger logger,
        IConsole console,
        IEnumerable<Uri> sources,
        string minUnityVersion,
        Uri target,
        string? repository,
        string? directory
    )
        : base(
            host: host,
            configuration: configuration,
            loggerFactory: loggerFactory,
            logger: logger,
            console: console,
            sources: sources,
            minUnityVersion: minUnityVersion,
            target: target,
            repository: repository,
            directory: directory
        ) { }

    public Context(Context other)
        : base(other) { }
}
