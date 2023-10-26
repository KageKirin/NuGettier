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
    public IEnumerable<Uri> Sources { get; protected set; }
    public SourceCacheContext Cache { get; protected set; }
    public IEnumerable<SourceRepository> Repositories { get; protected set; }
    public IConsole Console { get; set; }

    public Context(IEnumerable<Uri> sources, IConsole console)
    {
        Console = console;
        Sources = sources;
        Cache = new();

        Repositories = Sources.Select(source =>
        {
            var requestSource = source.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped);
            console.WriteLine($"adding source {requestSource}");
            string? username = null;
            string? password = null;
            if (!string.IsNullOrEmpty(source.UserInfo))
            {
                var userInfo = source.UserInfo.Split(':');
                username = userInfo[0];
                password = userInfo[1];
                console.WriteLine($"\tusername: {username}");
                console.WriteLine($"\tpassword: {password}");
            }
            PackageSource packageSource =
                new(requestSource, source.Authority)
                {
                    Credentials =
                        (username is not null && password is not null)
                            ? PackageSourceCredential.FromUserInput(
                                source: source.Authority,
                                username: username,
                                password: password,
                                storePasswordInClearText: true,
                                validAuthenticationTypesText: null
                            )
                            : null,
                };
            return NuGet.Protocol.Core.Types.Repository.Factory.GetCoreV3(packageSource);
        });
    }

    public void Dispose() { }
}
