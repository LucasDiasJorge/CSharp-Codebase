namespace ProblemDetailsApi.Domain;

/// <summary>
/// Base das falhas previstas do domínio. Cada subclasse carrega o status HTTP que lhe
/// corresponde e um <see cref="ErrorType"/> estável — a URI que identifica a categoria
/// do erro e que o cliente pode comparar sem depender do texto da mensagem.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message, int statusCode, string errorType, string title)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorType = errorType;
        Title = title;
    }

    public int StatusCode { get; }

    public string ErrorType { get; }

    public string Title { get; }

    /// <summary>
    /// Campos extras específicos da falha, anexados ao ProblemDetails como extensions.
    /// </summary>
    public virtual IReadOnlyDictionary<string, object?> Extensions => new Dictionary<string, object?>();
}
