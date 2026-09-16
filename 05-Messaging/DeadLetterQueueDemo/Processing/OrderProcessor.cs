using System.Text.Json;

namespace DeadLetterQueueDemo.Processing;

/// <summary>
/// Resultado do processamento. A distinção entre falha transitória e permanente é a
/// decisão mais importante do consumidor: retentar um erro permanente só gasta tempo e
/// atrasa a fila, e mandar um erro transitório direto para a DLQ descarta trabalho que
/// teria dado certo na segunda tentativa.
/// </summary>
public enum ProcessingOutcome
{
    Success,

    /// <summary>Banco fora, timeout, indisponibilidade — vale retentar.</summary>
    TransientFailure,

    /// <summary>Payload inválido, regra violada — retentar não muda nada.</summary>
    PermanentFailure
}

public sealed class ProcessingResult
{
    private ProcessingResult(ProcessingOutcome outcome, string detail)
    {
        Outcome = outcome;
        Detail = detail;
    }

    public ProcessingOutcome Outcome { get; }

    public string Detail { get; }

    public static ProcessingResult Success() => new ProcessingResult(ProcessingOutcome.Success, "processado");

    public static ProcessingResult Transient(string detail) => new ProcessingResult(ProcessingOutcome.TransientFailure, detail);

    public static ProcessingResult Permanent(string detail) => new ProcessingResult(ProcessingOutcome.PermanentFailure, detail);
}

/// <summary>
/// Processa o pedido. O comportamento é ditado pelo próprio payload, para que cada
/// cenário de falha possa ser disparado por uma requisição.
/// </summary>
public sealed class OrderProcessor
{
    // JsonSerializerDefaults.Web: o payload viaja em camelCase, e o padrão de
    // JsonSerializer é sensível a maiúsculas. Sem isto, TODA mensagem desserializa com
    // os campos vazios e cai como falha permanente — um erro silencioso, porque o JSON
    // em si é válido.
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(ILogger<OrderProcessor> logger)
    {
        _logger = logger;
    }

    public ProcessingResult Process(string payload, int attempt)
    {
        OrderMessage? order;

        try
        {
            order = JsonSerializer.Deserialize<OrderMessage>(payload, JsonOptions);
        }
        catch (JsonException ex)
        {
            // Payload que nao desserializa nunca vai desserializar. Retentar seria
            // repetir o mesmo erro tres vezes antes de desistir.
            return ProcessingResult.Permanent("payload invalido: " + ex.Message);
        }

        if (order is null || string.IsNullOrWhiteSpace(order.OrderId))
        {
            return ProcessingResult.Permanent("mensagem sem orderId");
        }

        switch (order.Behavior)
        {
            case "ok":
                _logger.LogInformation("Pedido {PedidoId} processado na tentativa {Tentativa}.", order.OrderId, attempt);

                return ProcessingResult.Success();

            case "transient":
                // Falha nas primeiras tentativas e passa depois: o caso que a retentativa
                // existe para resolver.
                if (attempt >= order.SucceedOnAttempt)
                {
                    _logger.LogInformation(
                        "Pedido {PedidoId} deu certo na tentativa {Tentativa}, como esperado.",
                        order.OrderId,
                        attempt);

                    return ProcessingResult.Success();
                }

                return ProcessingResult.Transient($"dependencia indisponivel (tentativa {attempt})");

            case "poison":
                // Sempre falha. Com requeue infinito, esta mensagem sozinha trava a fila.
                return ProcessingResult.Transient("falha permanente disfarcada de transitoria");

            case "invalid":
                return ProcessingResult.Permanent("regra de negocio violada: total negativo");

            default:
                return ProcessingResult.Permanent($"comportamento desconhecido: {order.Behavior}");
        }
    }
}

public sealed class OrderMessage
{
    public string OrderId { get; set; } = string.Empty;

    /// <summary>ok, transient, poison ou invalid.</summary>
    public string Behavior { get; set; } = "ok";

    /// <summary>Para o comportamento transient: em que tentativa passa a dar certo.</summary>
    public int SucceedOnAttempt { get; set; } = 2;
}
