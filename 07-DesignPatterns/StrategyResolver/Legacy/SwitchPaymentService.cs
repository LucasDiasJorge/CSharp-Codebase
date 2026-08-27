using Microsoft.Extensions.Logging;
using StrategyResolver.Domain;

namespace StrategyResolver.Legacy;

/// <summary>
/// Ponto de partida descrito no artigo: a decisão de qual algoritmo usar mora em um switch,
/// dentro do serviço que orquestra. Quatro problemas ficam visíveis aqui e desaparecem em
/// <c>PaymentProcessingService</c>:
/// 1. cada método de pagamento novo obriga a editar este arquivo (viola Open/Closed);
/// 2. o serviço precisa conhecer todas as implementações concretas;
/// 3. critério composto vira if aninhado dentro do case, misturando seleção e regra de negócio;
/// 4. as regras de cada método ficam no mesmo corpo, sem fronteira de teste.
/// Mantido apenas como contraste didático — não é o caminho recomendado.
/// </summary>
public sealed class SwitchPaymentService
{
    private readonly ILogger<SwitchPaymentService> _logger;

    public SwitchPaymentService(ILogger<SwitchPaymentService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ProcessAsync(IEnumerable<PaymentRequest> requests, CancellationToken cancellationToken)
    {
        foreach (PaymentRequest request in requests)
        {
            switch (request.Method)
            {
                case PaymentMethods.Pix:
                    _logger.LogInformation("[switch] PIX liquidado para o pedido {OrderId}.", request.OrderId);
                    break;

                case PaymentMethods.Boleto:
                    _logger.LogInformation("[switch] Boleto emitido para o pedido {OrderId}.", request.OrderId);
                    break;

                case PaymentMethods.CreditCard:
                    // O critério composto não cabe no rótulo do case: vira mais um if aqui dentro.
                    if (request.Installments > 1)
                    {
                        _logger.LogInformation(
                            "[switch] Cartão parcelado em {Installments}x para o pedido {OrderId}.",
                            request.Installments,
                            request.OrderId);
                    }
                    else
                    {
                        _logger.LogInformation("[switch] Cartão à vista para o pedido {OrderId}.", request.OrderId);
                    }

                    break;

                // Todo método novo entra como mais um case aqui dentro.
                default:
                    _logger.LogWarning(
                        "[switch] Método {Method} não previsto; pedido {OrderId} recusado.",
                        request.Method,
                        request.OrderId);
                    break;
            }
        }

        return Task.CompletedTask;
    }
}
