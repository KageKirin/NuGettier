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
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using NuGettier.Upm;
using Xunit;

namespace NuGettier;

public static partial class Program
{
    private static Command UpmMetagenCommand =>
        new Command("metagen", "generate .meta files for given files")
        {
            InputItemsArgument,
            UpmMetagenSeedOption,
            ForceOverwriteOption,
        }.WithHandler(CommandHandler.Create(UpmMetagen));

    private static async Task<int> UpmMetagen(
        IEnumerable<string> inputItems,
        string seed,
        bool force,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(nameof(UpmNpmrc));
        Assert.NotNull(Configuration);

        Logger.LogTrace("input items: {0}", inputItems);
        Logger.LogTrace("seed: {0}", seed);
        Logger.LogTrace("force: {0}", force);

        using MetaFactory metaFactory = new(seed, MainLoggerFactory);
        Matcher matcher = new();
        matcher.AddExclude(@"*.meta");
        matcher.AddIncludePatterns(inputItems);

        var files = matcher //
            .Execute(new DirectoryInfoWrapper(new DirectoryInfo(Environment.CurrentDirectory)))
            .Files;

        var folders = inputItems //
            .Where(Directory.Exists)
            .Concat(files.Select(file => Path.GetDirectoryName(file.Path)));

        var metaFiles = (
            await Task.WhenAll(
                files
                    .Where(file => File.Exists(file.Path))
                    .Select(
                        async (file) =>
                        {
                            FileInfo metaFile = new($"{file.Path}.meta");
                            if (!metaFile.Exists || force)
                            {
                                var metaContents = metaFactory.GenerateFileMeta(file.Path);
                                Logger.LogInformation("generated {0}", metaFile.FullName);
                                await File.WriteAllTextAsync(metaFile.FullName, metaContents);
                                metaFile.Refresh();
                            }
                            else
                            {
                                Logger.LogInformation("skipped existing metafile {0}", metaFile.FullName);
                            }
                            return metaFile;
                        }
                    )
            )
        ).ToList();

        metaFiles.AddRange(
            await Task.WhenAll(
                folders
                    .Where(folder => folder is not null)
                    .Where(Path.Exists)
                    .Select(
                        async (folder) =>
                        {
                            FileInfo metaFile = new($"{folder}.meta");
                            if (!metaFile.Exists || force)
                            {
                                var metaContents = metaFactory.GenerateFolderMeta(folder!);
                                Logger.LogInformation("generated {0}", metaFile.FullName);
                                await File.WriteAllTextAsync(metaFile.FullName, metaContents);
                                metaFile.Refresh();
                            }
                            else
                            {
                                Logger.LogInformation("skipped existing metafile {0}", metaFile.FullName);
                            }
                            return metaFile;
                        }
                    )
            )
        );

        Logger.LogInformation("generated {0} .meta files", metaFiles.Count);
        Logger.LogTrace("exit {0} command handler without error", nameof(UpmMetagen));
        return 0;
    }
}
