using CovidLetter.Frontend.Queue;
using CovidLetter.Frontend.WebApp.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CovidLetter.Frontend.WebApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .Configure<AppOptions>(configuration)
            .AddSingleton<FeatureToggle>();

        return services
            .Configure<QueueConfig>(configuration.GetSection(QueueConfig.Identifier))
            .Configure<SiteConfiguration>(configuration.GetSection(SiteConfiguration.Identifier));
    }
}