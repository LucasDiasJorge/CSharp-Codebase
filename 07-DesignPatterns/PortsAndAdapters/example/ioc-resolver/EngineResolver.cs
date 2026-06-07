using PortsAndAdapters.Domain;
using PortsAndAdapters.app_core;
using PortsAndAdapters.brazil_domain;
using PortsAndAdapters.europe_domain;
using PortsAndAdapters.usa_domain;

namespace IocResolver;

public static class EngineResolver
{
    public static IServiceCollection ConfigureEngines(this IServiceCollection services)
    {
        services.AddSingleton<IEngine, EngineBrazil>();
        services.AddSingleton<IEngine, EngineEurope>();
        services.AddSingleton<IEngine, EngineUsa>();
        services.AddSingleton<EngineRegistry>();
        return services;
    }

}