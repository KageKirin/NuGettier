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
        }

        if (token != null)
        {
            Logger.LogTrace("writing .npmrc to {0}", targetNpmrc.FullName);

            // format is "//${schemeless_registry}/:_authToken=${token}"
            using (var npmrcWriter = new StreamWriter(targetNpmrc.OpenWrite()))
                await npmrcWriter.WriteLineAsync($"//{Target.SchemelessUri()}:_authToken={token}");
        }
        else if (npmrc != null)
        {
            FileInfo sourceNpmrc = new(npmrc);

            if (sourceNpmrc.Exists)
            {
                Logger.LogTrace("copying {0} to {1}", sourceNpmrc.FullName, targetNpmrc.FullName);
                await sourceNpmrc.OpenRead().CopyToAsync(targetNpmrc.OpenWrite());
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

        Assert.True(targetNpmrc.Exists);
        return targetNpmrc;
    }
}
