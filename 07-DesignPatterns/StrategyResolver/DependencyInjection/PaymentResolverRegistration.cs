using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StrategyResolver.Resolvers;
using StrategyResolver.Services;

namespace StrategyResolver.DependencyInjection;

/// <summary>
/// Composition root do módulo de pagamentos, exposto como extension method — a forma
/// convencional de um módulo publicar seu próprio registro sem espalhar AddSingleton
/// pelo Startup da aplicação.
/// O registro é feito por varredura do assembly: qualquer classe que implemente
/// <see cref="IPaymentResolver"/> entra na coleção automaticamente. Criar um método de
/// pagamento novo passa a ser criar uma classe, sem editar este arquivo.
/// </summary>
public static class PaymentResolverRegistration
{
    public static IServiceCollection AddPaymentResolvers(
        this IServiceCollection services,
        Func<Type, bool>? typeFilter = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        Assembly assembly = typeof(PaymentResolverRegistration).Assembly;

        IEnumerable<Type> resolverTypes = assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => typeof(IPaymentResolver).IsAssignableFrom(type))
            // O fallback não é candidato: seu AppliesTo sempre verdadeiro venceria qualquer
            // disputa em que fosse avaliado primeiro. Ele é registrado à parte, abaixo.
            .Where(type => type != typeof(UnsupportedPaymentResolver))
            // O filtro existe apenas para a demonstração do Program, onde ele simula o
            // assembly antes de a classe DigitalWalletResolver existir. Em produção,
            // chame AddPaymentResolvers() sem argumento.
            .Where(type => typeFilter is null || typeFilter(type));

        foreach (Type resolverType in resolverTypes)
        {
            services.AddSingleton(typeof(IPaymentResolver), resolverType);
        }

        // Fallback registrado pelo tipo concreto, fora da coleção de candidatos.
        services.AddSingleton<UnsupportedPaymentResolver>();

        services.AddSingleton<IPaymentProcessingService, PaymentProcessingService>();

        return services;
    }
}
