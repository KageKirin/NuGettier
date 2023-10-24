using System;
using System.IO;
using System.CommandLine;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGettier.Core;

#nullable enable

public partial class Context : IDisposable
{
    public Uri Source { get; protected set; }
    public SourceCacheContext Cache { get; protected set; }
    public SourceRepository Repository { get; protected set; }
    public IConsole Console { get; set; }

    public Context(Uri source, string? username, string? password, IConsole console)
    {
        console.WriteLine($"username: {username}");
        console.WriteLine($"password: {password}");
        this.Source = source;
        this.Cache = new();
        PackageSource packageSource =
            new(source.ToString(), @"source")
            {
                Credentials =
                    (username is not null && password is not null)
                        ? PackageSourceCredential.FromUserInput(
                            source: @"source",
                            username: username,
                            password: password,
                            storePasswordInClearText: true,
                            validAuthenticationTypesText: null
                        )
                        : null,
            };

        this.Repository = NuGet.Protocol.Core.Types.Repository.Factory.GetCoreV3(packageSource);
        this.Console = console;
    }

    public void Dispose() { }
}
