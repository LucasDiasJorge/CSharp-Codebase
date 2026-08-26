using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StrategyResolver.Legacy;
using StrategyResolver.Resolvers;
using StrategyResolver.Services;

namespace StrategyResolver;

/// <summary>
/// Composition root do exemplo — equivale ao <c>ConfigureServices</c> do artigo.
/// Registrar várias implementações do mesmo contrato faz o container entregar a coleção
/// inteira em <c>IEnumerable&lt;IFoodCookingResolver&gt;</c>, que é como o orquestrador
/// recebe seus candidatos. Ligar ou desligar um método de cocção é mexer só aqui.
/// </summary>
public static class CookingModule
{
    public static ServiceProvider BuildProvider(bool withSousVideResolver)
    {
        ServiceCollection services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // A ordem destas linhas é a ordem de avaliação de AppliesTo no serviço.
        services.AddSingleton<IFoodCookingResolver, OvenResolver>();
        services.AddSingleton<IFoodCookingResolver, GrillResolver>();

        if (withSousVideResolver)
        {
            services.AddSingleton<IFoodCookingResolver, SousVideResolver>();
        }

        // Fallback registrado pelo tipo concreto: fica fora da coleção acima de propósito,
        // para que o AppliesTo sempre verdadeiro do DefaultResolver não engula os demais.
        services.AddSingleton<DefaultResolver>();

        services.AddSingleton<IDishResolutionService, DishResolutionService>();
        services.AddSingleton<SwitchCookingService>();

        return services.BuildServiceProvider();
    }
}
