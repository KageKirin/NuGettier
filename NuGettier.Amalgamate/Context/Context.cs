using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Core.Types;
using NuGettier;

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
        IConsole console,
        ILoggerFactory loggerFactory
    )
        : this(
            configuration: configuration,
            sources: sources,
            minUnityVersion: minUnityVersion,
            target: target,
            repository: repository,
            directory: directory,
            console: console,
            loggerFactory: loggerFactory,
            logger: loggerFactory.CreateLogger<Amalgamate.Context>()
        ) { }

    protected Context(
        IConfigurationRoot configuration,
        IEnumerable<Uri> sources,
        string minUnityVersion,
        Uri target,
        string? repository,
        string? directory,
        IConsole console,
        ILoggerFactory loggerFactory,
        ILogger logger
    )
        : base(
            configuration: configuration,
            sources: sources,
            minUnityVersion: minUnityVersion,
            target: target,
            repository: repository,
            directory: directory,
            console: console,
            loggerFactory: loggerFactory,
            logger: logger
        ) { }

    public Context(Context other)
        : base(other) { }
}
