using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;
using StrategyResolver.Resolvers;

namespace StrategyResolver.Services;

/// <summary>
/// Orquestrador do Advanced Resolver Pattern. Este arquivo é o que nunca mais muda:
/// resolve cada pedido, agrupa por resolver vencedor e dispara os lotes em paralelo.
/// Não há switch, nem if por método de pagamento, nem referência a implementação concreta
/// além do fallback.
/// </summary>
public sealed class PaymentProcessingService : IPaymentProcessingService
{
    private readonly IReadOnlyList<IPaymentResolver> _resolvers;
    private readonly UnsupportedPaymentResolver _fallbackResolver;
    private readonly ILogger<PaymentProcessingService> _logger;

    public PaymentProcessingService(
        IEnumerable<IPaymentResolver> resolvers,
        UnsupportedPaymentResolver fallbackResolver,
        ILogger<PaymentProcessingService> logger)
    {
        ArgumentNullException.ThrowIfNull(resolvers);

        // A ordenação acontece uma única vez, na construção. É ela que torna a precedência
        // um contrato do resolver: o auto-registro por scanning não garante ordem alguma,
        // então depender da sequência de AddSingleton seria depender de acaso.
        _resolvers = resolvers.OrderBy(resolver => resolver.Priority).ToList();

        _fallbackResolver = fallbackResolver ?? throw new ArgumentNullException(nameof(fallbackResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessAsync(IEnumerable<PaymentRequest> requests, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requests);

        // Agrupar pelo resolver vencedor, e não pelo método de pagamento: com critério
        // composto em AppliesTo, dois pedidos do mesmo método podem cair em resolvers
        // diferentes (cartão à vista e cartão parcelado). Resolvers são singletons,
        // então a igualdade por referência é suficiente como chave de agrupamento.
        IEnumerable<IGrouping<IPaymentResolver, PaymentRequest>> batches =
            requests.GroupBy(request => SelectResolver(request));

        List<Task> processingTasks = new List<Task>();

        foreach (IGrouping<IPaymentResolver, PaymentRequest> batch in batches)
        {
            IReadOnlyCollection<PaymentRequest> batchRequests = batch.ToList();

            _logger.LogInformation(
                "{ResolverName} assumiu {RequestCount} pedido(s): {OrderIds}.",
                batch.Key.GetType().Name,
                batchRequests.Count,
                string.Join(", ", batchRequests.Select(request => request.OrderId)));

            processingTasks.Add(batch.Key.ProcessAsync(batchRequests, cancellationToken));
        }

        await Task.WhenAll(processingTasks);
    }

    /// <summary>
    /// O coração do padrão. Percorre os candidatos já ordenados por prioridade e fica com o
    /// primeiro que se declarar aplicável; se nenhum aceitar, cai no fallback explícito.
    /// Adicionar um método de pagamento novo não toca neste método.
    /// </summary>
    private IPaymentResolver SelectResolver(PaymentRequest request)
    {
        IPaymentResolver? resolver = _resolvers.FirstOrDefault(candidate => candidate.AppliesTo(request));
        return resolver ?? _fallbackResolver;
    }
}
