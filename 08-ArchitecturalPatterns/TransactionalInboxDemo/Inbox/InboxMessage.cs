namespace TransactionalInboxDemo.Inbox;

/// <summary>
/// Registro de que uma mensagem já foi processada. A chave é o <see cref="MessageId"/>
/// que veio do produtor — não um id gerado aqui, que não serviria para reconhecer a
/// mesma mensagem chegando de novo.
/// </summary>
public sealed class InboxMessage
{
    public InboxMessage(Guid messageId, string type)
    {
        MessageId = messageId;
        Type = type;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    private InboxMessage()
    {
        Type = string.Empty;
    }

    public int Id { get; private set; }

    public Guid MessageId { get; private set; }

    public string Type { get; private set; }

    public DateTimeOffset ProcessedAt { get; private set; }
}
