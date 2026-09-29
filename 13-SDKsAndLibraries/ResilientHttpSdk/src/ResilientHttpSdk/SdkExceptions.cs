using System.Net;

namespace ResilientHttpSdk;

/// <summary>
/// A raiz da hierarquia de erros do SDK.
///
/// Um SDK que deixa escapar <see cref="HttpRequestException"/> e código de status obriga
/// cada chamador a reaprender HTTP. Traduzir para tipos próprios é o que transforma a
/// biblioteca em uma API, e não em um invólucro de <c>HttpClient</c>.
/// </summary>
public abstract class SdkException : Exception
{
    protected SdkException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// O status que originou o erro, quando houve resposta. Nulo em falha de transporte.
    /// Exposto porque esconder totalmente prejudica diagnóstico — o que não se quer é
    /// <b>obrigar</b> o chamador a olhar.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }
}

/// <summary>A requisição foi rejeitada por conteúdo inválido (400, 422).</summary>
public sealed class SdkValidationException : SdkException
{
    public SdkValidationException(string message, HttpStatusCode statusCode, IReadOnlyDictionary<string, string[]> errors)
        : base(message, statusCode)
    {
        Errors = errors;
    }

    /// <summary>Erros por campo, quando a resposta trouxe <c>ProblemDetails</c>.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

/// <summary>O recurso não existe (404).</summary>
public sealed class SdkNotFoundException : SdkException
{
    public SdkNotFoundException(string message)
        : base(message, HttpStatusCode.NotFound)
    {
    }
}

/// <summary>Credencial ausente, inválida ou sem permissão (401, 403).</summary>
public sealed class SdkAuthenticationException : SdkException
{
    public SdkAuthenticationException(string message, HttpStatusCode statusCode)
        : base(message, statusCode)
    {
    }
}

/// <summary>
/// Limite de uso excedido (429). Carrega o <c>Retry-After</c> quando o servidor informa —
/// sem isso o chamador só pode adivinhar quando tentar de novo.
/// </summary>
public sealed class SdkRateLimitException : SdkException
{
    public SdkRateLimitException(string message, TimeSpan? retryAfter)
        : base(message, HttpStatusCode.TooManyRequests)
    {
        RetryAfter = retryAfter;
    }

    public TimeSpan? RetryAfter { get; }
}

/// <summary>Falha do outro lado (5xx). Já passou pelas tentativas de retry.</summary>
public sealed class SdkServerException : SdkException
{
    public SdkServerException(string message, HttpStatusCode statusCode)
        : base(message, statusCode)
    {
    }
}

/// <summary>
/// Falha de transporte: DNS, conexão recusada, TLS, ou <b>timeout do SDK</b>.
///
/// Distinguir isto de cancelamento do chamador é o ponto mais fácil de errar: os dois
/// chegam como <see cref="OperationCanceledException"/>.
/// </summary>
public sealed class SdkTransportException : SdkException
{
    public SdkTransportException(string message, Exception? innerException = null)
        : base(message, statusCode: null, innerException)
    {
    }
}
