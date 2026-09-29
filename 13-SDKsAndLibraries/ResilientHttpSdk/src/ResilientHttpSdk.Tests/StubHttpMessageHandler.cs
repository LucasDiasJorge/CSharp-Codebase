using System.Net;
using System.Text;

namespace ResilientHttpSdk.Tests;

/// <summary>
/// Handler de teste no fim do pipeline: devolve respostas programadas e conta tentativas.
///
/// É a única forma de testar um SDK HTTP sem rede. O contador de chamadas é o que permite
/// afirmar quantas tentativas o retry fez — sem ele, "tem retry" é fé.
/// </summary>
public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();
    private readonly List<HttpRequestMessage> _requests = new();

    public IReadOnlyList<HttpRequestMessage> Requests => _requests;

    public int CallCount => _requests.Count;

    public StubHttpMessageHandler Respond(HttpStatusCode status, string body = "", Action<HttpResponseMessage>? customize = null)
    {
        _responses.Enqueue(_ =>
        {
            HttpResponseMessage response = new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };

            customize?.Invoke(response);

            return response;
        });

        return this;
    }

    public StubHttpMessageHandler RespondWith(Func<HttpRequestMessage, HttpResponseMessage> factory)
    {
        _responses.Enqueue(factory);

        return this;
    }

    /// <summary>Simula falha de transporte (DNS, conexão recusada, TLS).</summary>
    public StubHttpMessageHandler Throw(Exception exception)
    {
        _responses.Enqueue(_ => throw exception);

        return this;
    }

    /// <summary>Demora mais que o timeout configurado, respeitando o cancelamento.</summary>
    public StubHttpMessageHandler Delay(TimeSpan delay)
    {
        _responses.Enqueue(_ =>
        {
            Task.Delay(delay).GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _requests.Add(request);

        cancellationToken.ThrowIfCancellationRequested();

        // Fila vazia: repete a ultima resposta programada, para nao obrigar a enfileirar
        // uma resposta por tentativa de retry.
        Func<HttpRequestMessage, HttpResponseMessage> factory = _responses.Count > 0
            ? (_responses.Count == 1 ? _responses.Peek() : _responses.Dequeue())
            : _ => new HttpResponseMessage(HttpStatusCode.OK);

        return Task.FromResult(factory(request));
    }
}
