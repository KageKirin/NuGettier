using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NuGettier.Upm;

public class GuidFactoryProxy : IGuidFactory, IDisposable
{
    const string kDefaultIdentifier = "sha1";

    protected readonly ILogger Logger;
    protected readonly IGuidFactory GuidFactory;

    public GuidFactoryProxy(
        ILogger<GuidFactoryProxy> logger,
        IServiceProvider serviceProvider,
        IOptions<GuidFactorySettings> options
    )
    {
        Logger = logger;

        var identifier = !string.IsNullOrEmpty(options?.Value.Algorithm) ? options!.Value.Algorithm : kDefaultIdentifier;
        logger.LogTrace("identifier: {0}", identifier);
        GuidFactory =
            serviceProvider.GetKeyedService<IGuidFactory>(identifier)
            ?? serviceProvider.GetRequiredKeyedService<IGuidFactory>(kDefaultIdentifier);
    }

    public virtual void Dispose() { }

    public virtual void InitializeWithSeed(string seed) => GuidFactory.InitializeWithSeed(seed);

    public virtual Guid GenerateGuid(string value) => GuidFactory.GenerateGuid(value);
}
