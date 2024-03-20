using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NuGettier;

public partial class NuGettierService
{
    private Command UpmNpmrcCommand =>
        new Command("npmrc", "generate .npmrc in given folder")
        {
            TargetRegistryOption,
            OutputDirectoryOption,
            UpmTokenOption,
            UpmNpmrcOption,
        }.WithHandler(CommandHandler.Create(UpmNpmrc));

    private async Task<int> UpmNpmrc(
        Uri target,
        DirectoryInfo outputDirectory,
        string? token,
        string? npmrc,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(nameof(UpmNpmrc));
        Assert.NotNull(Configuration);
        using var context = new Upm.Context(
            configuration: Configuration,
            loggerFactory: MainLoggerFactory,
            logger: MainLoggerFactory.CreateLogger<Upm.Context>(),
            console: console,
            sources: Array.Empty<Uri>(),
            minUnityVersion: (UpmUnityVersionOption as IValueDescriptor<string>).GetDefaultValue() as string
                ?? string.Empty,
            target: target,
            repository: null,
            directory: null
        );
        var fileInfo = await context.GenerateNpmrc(
            outputDirectory: outputDirectory,
            token: token,
            npmrc: npmrc,
            cancellationToken: cancellationToken
        );

        fileInfo.Refresh();
        if (!fileInfo.Exists)
        {
            Logger.LogError("failed to generate .npmrc {0}", fileInfo.FullName);
            return 1;
        }

        Logger.LogInformation("wrote .npmrc {0}", fileInfo.FullName);
        Logger.LogTrace("exit {0} command handler without error", nameof(UpmNpmrc));
        return 0;
    }
}
