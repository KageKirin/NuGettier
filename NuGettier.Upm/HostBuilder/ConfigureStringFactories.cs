using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NuGettier.Upm;

public static class ConfigureStringFactoriesExtensions
{
    public static IHostBuilder ConfigureStringFactories(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices(
            (context, services) =>
            {
                services.AddOptions();
                services.AddScoped<INpmrcFactory, NpmrcFactory>();
                services.AddScoped<IReadmeFactory, ReadmeFactory>();
                services.AddScoped<ILicenseFactory, LicenseFactory>();
                services.AddScoped<IChangelogFactory, ChangelogFactory>();
                services.AddScoped<IMetaFactory, MetaFactory>();
                services.AddScoped<IGuidFactory, GuidFactoryProxy>();

                Type interfaceType = typeof(IGuidFactory);
                foreach (
                    Type type in Assembly
                        .GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => t.GetInterfaces().Contains(interfaceType))
                )
                {
                    var attribute = type.GetCustomAttribute<GuidIdentifierAttribute>();
                    if (attribute is not null)
                    {
                        services.AddKeyedScoped(interfaceType, attribute.Identifier, type);
                    }
                }

                services.AddOptions<GuidFactorySettings>().Bind(context.Configuration.GetSection("guid"));
            }
        );
    }
}
