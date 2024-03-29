using System;
using System.Reflection;
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
    protected readonly IGuidFormatter GuidFormatter;

    public GuidFactoryProxy(
        ILogger<GuidFactoryProxy> logger,
        IServiceProvider serviceProvider,
        IOptions<GuidFactorySettings> options
    )
    {
        Logger = logger;

        var identifier = !string.IsNullOrEmpty(options?.Value.Algorithm)
            ? options!.Value.Algorithm
            : Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Select(t => t.GetCustomAttribute<GuidIdentifierAttribute>())
                .First()
                ?.Identifier ?? kDefaultIdentifier;
        ;
        logger.LogTrace("using Guid algorithm: {0}", identifier);
        GuidFactory =
            serviceProvider.GetKeyedService<IGuidFactory>(identifier)
            ?? serviceProvider.GetRequiredKeyedService<IGuidFactory>(kDefaultIdentifier);

        logger.LogTrace("using Guid formatter: {0}", options?.Value.Format ?? GuidFormat.None);
        GuidFormatter =
            serviceProvider.GetKeyedService<IGuidFormatter>(options?.Value.Format ?? GuidFormat.None)
            ?? serviceProvider.GetRequiredKeyedService<IGuidFormatter>(GuidFormat.None);
    }

    public virtual void Dispose() { }

    public virtual void InitializeWithSeed(string seed) => GuidFactory.InitializeWithSeed(seed);

    public virtual Guid GenerateGuid(string value) => GuidFormatter.Apply(GuidFactory.GenerateGuid(value));
}
