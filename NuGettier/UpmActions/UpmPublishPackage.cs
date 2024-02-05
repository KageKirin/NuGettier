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

public static partial class Program
{
    private static Command UpmPublishPackageCommand =>
        new Command("publish-package", "publish a pre-packed UPM package")
        {
            PackageFileArgument,
            TargetRegistryOption,
            UpmDirectoryUrlOption,
            UpmTokenOption,
            UpmNpmrcOption,
            UpmDryRunOption,
            UpmTimeOutOption,
            UpmPackageAccessLevel,
        }.WithHandler(CommandHandler.Create(UpmPublishPackage));

    private static async Task<int> UpmPublishPackage(
        FileInfo packageFile,
        Uri target,
        string unity,
        string? directory,
        string? token,
        string? npmrc,
        bool dryRun,
        int timeOut,
        Upm.PackageAccessLevel access,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(nameof(UpmPublishPackage));
        Assert.NotNull(Configuration);
        using var context = new Upm.Context(
            configuration: Configuration!,
            sources: Array.Empty<Uri>(),
            minUnityVersion: (UpmUnityVersionOption as IValueDescriptor<string>).GetDefaultValue() as string
                ?? string.Empty,
            target: target,
            repository: null,
            directory: null,
            console: console,
            loggerFactory: MainLoggerFactory
        );
        var result = await context.PublishPackedUpmPackage(
            packageFile: packageFile,
            token: token,
            npmrc: npmrc,
            dryRun: dryRun,
            timeOut: timeOut,
            packageAccessLevel: access,
            cancellationToken: cancellationToken
        );

        if (result != 0)
        {
            Logger.LogError("failed to publish UPM package {0}", packageFile.FullName);
        }

        Logger.LogTrace("exit {0} command handler with error {1}", nameof(UpmPublishPackage), result);
        return result;
    }
}
