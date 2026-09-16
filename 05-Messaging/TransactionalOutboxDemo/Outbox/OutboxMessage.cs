namespace TransactionalOutboxDemo.Outbox;

/// <summary>
/// Mensagem à espera de publicação, gravada na mesma transação da entidade que a
/// originou. É esta linha na mesma tabela-mãe — o mesmo banco, o mesmo commit — que
/// elimina o dual write.
/// </summary>
public sealed class OutboxMessage
{
    public OutboxMessage(string type, string payload)
    {
        MessageId = Guid.NewGuid();
        Type = type;
        Payload = payload;
        OccurredAt = DateTimeOffset.UtcNow;
    }

    private OutboxMessage()
    {
        Type = string.Empty;
        Payload = string.Empty;
    }

    public int Id { get; private set; }

    /// <summary>
    /// Identidade estável da mensagem, gerada junto com ela e preservada em toda
    /// retentativa. É o que permite ao consumidor descartar duplicatas — a entrega é
    /// pelo menos uma vez, então duplicata vai acontecer.
    /// </summary>
    public Guid MessageId { get; private set; }

    public string Type { get; private set; }

    public string Payload { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public int AttemptCount { get; private set; }

    public string? LastError { get; private set; }

    public bool IsPending => PublishedAt is null;

    public void MarkPublished()
    {
        PublishedAt = DateTimeOffset.UtcNow;
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        AttemptCount++;
        LastError = error;
    }
}
