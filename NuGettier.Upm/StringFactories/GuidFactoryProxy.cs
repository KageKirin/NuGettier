using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NuGettier.Upm;

public class GuidFactoryProxy : IGuidFactory, IDisposable
{
    protected readonly ILogger Logger;
    protected readonly IGuidFactory GuidFactory;

    public GuidFactoryProxy(
        ILogger<GuidFactoryProxy> logger,
        IServiceProvider serviceProvider,
        IOptions<GuidFactorySettings> options
    )
    {
        Logger = logger;

        var algorithm = !string.IsNullOrEmpty(options?.Value.Algorithm) ? options!.Value.Algorithm : "sha1";
        logger.LogTrace("algorithm: {0}", algorithm);
        GuidFactory =
            serviceProvider.GetKeyedService<IGuidFactory>(algorithm)
            ?? serviceProvider.GetRequiredKeyedService<IGuidFactory>("sha1");
    }

    public virtual void Dispose() { }

    public virtual void InitializeWithSeed(string seed) => GuidFactory.InitializeWithSeed(seed);

    public virtual Guid GenerateGuid(string value) => GuidFactory.GenerateGuid(value);
}
