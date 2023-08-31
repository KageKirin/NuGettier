using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGettier;

public static partial class Program
{
    private static Command SearchCommand =>
        new Command("search", "search for term or package name")
        {
            SearchTermArgument,
            OutputJsonOption,
            SourceRepositoryOption,
        }.WithHandler(CommandHandler.Create(Search));

    private static async Task<int> Search(
        string searchTerm,
        bool json,
        Uri source,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var context = new Core.Context(source: source, console: console);
        return await context.SearchPackages(
            searchTerm: searchTerm,
            json: json,
            cancellationToken: cancellationToken
        );
    }
}
