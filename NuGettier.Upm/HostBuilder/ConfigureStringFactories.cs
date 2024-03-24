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
                services.AddKeyedScoped<IGuidFactory, Sha1GuidFactory>("sha1");
                services.AddKeyedScoped<IGuidFactory, Md5GuidFactory>("md5");
                services.AddKeyedScoped<IGuidFactory, XxHash128GuidFactory>("xxhash128");
                services.AddKeyedScoped<IGuidFactory, XxHash3GuidFactory>("xxhash3");
                services.AddKeyedScoped<IGuidFactory, XxHash64GuidFactory>("xxhash64");
                services.AddKeyedScoped<IGuidFactory, Upm.Uranium.XxHash128GuidFactory>("uranium.xxhash128");
                services.AddKeyedScoped<IGuidFactory, Upm.Uranium.XxHash3GuidFactory>("uranium.xxhash3");
                services.AddKeyedScoped<IGuidFactory, Upm.Uranium.XxHash64GuidFactory>("uranium.xxhash64");
                services.AddOptions<GuidFactorySettings>().Bind(context.Configuration.GetSection("guid"));
            }
        );
    }
}
