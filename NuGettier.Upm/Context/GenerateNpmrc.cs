using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGettier.Upm.TarGz;
using Xunit;

namespace NuGettier.Upm;

#nullable enable

public partial class Context
{
    public virtual async Task<FileInfo> GenerateNpmrc(
        DirectoryInfo outputDirectory,
        string? token,
        string? npmrc,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        Logger.LogTrace("creating .npmrc package in {0}", outputDirectory.FullName);

        FileInfo targetNpmrc = new(Path.Join(outputDirectory.FullName, @".npmrc"));

        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }

        if (targetNpmrc.Exists)
        {
            Logger.LogTrace("deleting existing .npmrc {0}", targetNpmrc.FullName);
            targetNpmrc.Delete();
            targetNpmrc.Refresh();
        }

        if (!string.IsNullOrEmpty(token))
        {
            Logger.LogTrace("writing .npmrc to {0}", targetNpmrc.FullName);

            using (var serviceScope = Host.Services.CreateScope())
            {
                NpmrcFactory npmrcFactory = serviceScope.ServiceProvider.GetRequiredService<NpmrcFactory>();
                await File.WriteAllTextAsync(targetNpmrc.FullName, npmrcFactory.GenerateNpmrc(Target, token));
            }
            targetNpmrc.Refresh();
        }
        else if (!string.IsNullOrEmpty(npmrc))
        {
            FileInfo sourceNpmrc = new(npmrc);
            if (sourceNpmrc.Exists)
            {
                Logger.LogTrace("copying {0} to {1}", sourceNpmrc.FullName, targetNpmrc.FullName);
                sourceNpmrc.CopyTo(targetNpmrc.FullName, overwrite: true); //< keeping sync copy here results in less code
                targetNpmrc.Refresh();
            }
            else
            {
                Logger
                    .TraceLocation()
                    .LogError(
                        "source file {0} does not exist, failed to copy to {1}",
                        sourceNpmrc.FullName,
                        targetNpmrc.FullName
                    );
            }
        }
        else
        {
            Logger.LogTrace("writing .npmrc to {0}", targetNpmrc.FullName);

            using (NpmrcFactory npmrcFactory = new())
                await File.WriteAllTextAsync(targetNpmrc.FullName, npmrcFactory.GenerateNpmrc(Target, string.Empty));
            targetNpmrc.Refresh();
        }

        targetNpmrc.Refresh();
        if (targetNpmrc.Exists)
        {
            Logger.LogInformation(
                "generated {0} with following contents:\n{1}",
                targetNpmrc.FullName,
                await File.ReadAllTextAsync(targetNpmrc.FullName, cancellationToken)
            );
        }
        else
        {
            Logger.TraceLocation().LogWarning("failed to write {0}", targetNpmrc.FullName);
        }
        Assert.True(targetNpmrc.Exists);
        return targetNpmrc;
    }
}
