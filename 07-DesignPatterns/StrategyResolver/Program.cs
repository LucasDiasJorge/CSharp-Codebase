using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StrategyResolver.DependencyInjection;
using StrategyResolver.Domain;
using StrategyResolver.Legacy;
using StrategyResolver.Resolvers;
using StrategyResolver.Services;

namespace StrategyResolver;

public static class Program
{
    public static async Task Main(string[] args)
    {
        PaymentRequest[] lote =
        [
            new PaymentRequest("PED-1001", PaymentMethods.Pix, 149.90m, 1),
            new PaymentRequest("PED-1002", PaymentMethods.Boleto, 320.00m, 1),
            new PaymentRequest("PED-1003", PaymentMethods.CreditCard, 890.00m, 1),
            new PaymentRequest("PED-1004", PaymentMethods.CreditCard, 2400.00m, 6),
            new PaymentRequest("PED-1005", PaymentMethods.DigitalWallet, 75.50m, 1)
        ];

        CancellationToken cancellationToken = CancellationToken.None;

        // Etapa 1 - o ponto de partida: a decisão vive num switch dentro do serviço.
        PrintHeader("1. Switch dentro do orquestrador (o que o padrão substitui)");
        await using (ServiceProvider legacyProvider = BuildProvider(includeDigitalWallet: false))
        {
            SwitchPaymentService switchService = legacyProvider.GetRequiredService<SwitchPaymentService>();
            await switchService.ProcessAsync(lote, cancellationToken);
        }

        // Etapa 2 - resolvers auto-registrados, simulando o assembly antes da carteira digital existir.
        // PED-1005 não tem dono e cai no fallback; PED-1004 mostra a prioridade em ação.
        PrintHeader("2. Auto-registro por scanning, sem DigitalWalletResolver no assembly");
        await using (ServiceProvider provider = BuildProvider(includeDigitalWallet: false))
        {
            await DescribePipelineAsync(provider, lote, cancellationToken);
        }

        // Etapa 3 - a classe DigitalWalletResolver passa a existir. Nenhuma linha mudou em
        // PaymentProcessingService nem em PaymentResolverRegistration: o scanning a encontra sozinho.
        PrintHeader("3. DigitalWalletResolver presente no assembly");
        await using (ServiceProvider provider = BuildProvider(includeDigitalWallet: true))
        {
            await DescribePipelineAsync(provider, lote, cancellationToken);
        }
    }

    private static ServiceProvider BuildProvider(bool includeDigitalWallet)
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

        // Uma linha registra o módulo inteiro. O filtro só existe para a demonstração:
        // em produção a chamada é services.AddPaymentResolvers().
        services.AddPaymentResolvers(
            typeFilter: includeDigitalWallet ? null : type => type != typeof(DigitalWalletResolver));

        services.AddSingleton<SwitchPaymentService>();

        return services.BuildServiceProvider();
    }

    private static async Task DescribePipelineAsync(
        ServiceProvider provider,
        IReadOnlyCollection<PaymentRequest> requests,
        CancellationToken cancellationToken)
    {
        IEnumerable<IPaymentResolver> registered = provider.GetServices<IPaymentResolver>()
            .OrderBy(resolver => resolver.Priority);

        Console.WriteLine("Resolvers na ordem de avaliação (prioridade crescente):");

        foreach (IPaymentResolver resolver in registered)
        {
            Console.WriteLine($"  [{resolver.Priority,4}] {resolver.GetType().Name}");
        }

        Console.WriteLine();

        IPaymentProcessingService processingService = provider.GetRequiredService<IPaymentProcessingService>();
        await processingService.ProcessAsync(requests, cancellationToken);
    }

    private static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('-', 78));
        Console.WriteLine(title);
        Console.WriteLine(new string('-', 78));
    }
}
