namespace DeadLetterQueueDemo.Messaging;

/// <summary>
/// Nomes da topologia. Três filas com papéis distintos, e é a separação entre elas que
/// permite limitar retentativas sem travar o consumo.
/// </summary>
public static class QueueTopology
{
    /// <summary>Exchange por onde tudo entra.</summary>
    public const string MainExchange = "orders";

    /// <summary>Fila de trabalho. O consumidor lê daqui.</summary>
    public const string MainQueue = "orders.main";

    /// <summary>
    /// Fila de espera. Mensagens ficam aqui pelo TTL e voltam sozinhas para a principal,
    /// porque a DLX desta fila aponta de volta para a exchange principal. É o jeito de
    /// obter retentativa com atraso no RabbitMQ sem plugin.
    /// </summary>
    public const string RetryExchange = "orders.retry";

    public const string RetryQueue = "orders.retry.queue";

    /// <summary>Estacionamento final: nada sai daqui sem decisão humana.</summary>
    public const string DeadLetterExchange = "orders.dlx";

    public const string DeadLetterQueue = "orders.dlq";

    public const string RoutingKey = "order";

    /// <summary>Quantas vezes uma falha transitória é retentada antes de ir para a DLQ.</summary>
    public const int MaxAttempts = 3;

    /// <summary>Atraso da fila de espera, em milissegundos.</summary>
    public const int RetryDelayMs = 3000;

    /// <summary>Cabeçalho que carrega a contagem de tentativas entre as filas.</summary>
    public const string AttemptHeader = "x-attempt";

    /// <summary>Cabeçalho com o motivo do descarte, preenchido ao mandar para a DLQ.</summary>
    public const string ReasonHeader = "x-death-reason";
}
