using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ResilientHttpSdk;

/// <summary>
/// O cliente tipado. Recebe <see cref="HttpClient"/> por construtor — quem cria e
/// descarta é o <c>IHttpClientFactory</c>, não o SDK.
///
/// A responsabilidade desta classe é uma só: transformar resposta HTTP em resultado ou
/// em exceção do SDK. Retry e timeout ficam no pipeline de handlers, fora daqui.
/// </summary>
public sealed class ProductsClient : IProductsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public ProductsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<Product> GetAsync(string id, CancellationToken cancellationToken = default) =>
        SendAsync(() => new HttpRequestMessage(HttpMethod.Get, $"products/{id}"), cancellationToken);

    public Task<Product> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default) =>
        SendAsync(
            () => new HttpRequestMessage(HttpMethod.Post, "products")
            {
                Content = JsonContent.Create(request, options: JsonOptions),
            },
            cancellationToken);

    private async Task<Product> SendAsync(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(requestFactory(), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // O CHAMADOR cancelou. Propagar como cancelamento é obrigatório: transformar
            // isso em erro do SDK faz `catch (OperationCanceledException)` do chamador
            // parar de funcionar, e é um dos bugs mais comuns em SDK.
            throw;
        }
        catch (OperationCanceledException exception)
        {
            // O token do chamador NÃO foi cancelado: quem estourou foi o timeout interno.
            // São a mesma exceção com significados diferentes.
            throw new SdkTransportException("a requisicao excedeu o tempo limite do SDK", exception);
        }
        catch (HttpRequestException exception)
        {
            throw new SdkTransportException($"falha de transporte: {exception.Message}", exception);
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                Product? product = await response.Content
                    .ReadFromJsonAsync<Product>(JsonOptions, cancellationToken)
                    .ConfigureAwait(false);

                return product ?? throw new SdkTransportException("a resposta veio vazia onde se esperava um produto");
            }

            throw await TranslateAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// A tradução: um status HTTP vira um tipo de exceção do SDK, com a mensagem do corpo
    /// quando houver <c>ProblemDetails</c>.
    /// </summary>
    private static async Task<SdkException> TranslateAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        ProblemDetailsPayload? problem = TryReadProblem(body);
        string detail = problem?.Detail ?? problem?.Title ?? (body.Length == 0 ? response.ReasonPhrase ?? "sem detalhe" : body);

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity =>
                new SdkValidationException(detail, response.StatusCode, problem?.Errors ?? new Dictionary<string, string[]>()),

            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                new SdkAuthenticationException(detail, response.StatusCode),

            HttpStatusCode.NotFound =>
                new SdkNotFoundException(detail),

            HttpStatusCode.TooManyRequests =>
                new SdkRateLimitException(detail, response.Headers.RetryAfter?.Delta),

            _ when (int)response.StatusCode >= 500 =>
                new SdkServerException(detail, response.StatusCode),

            _ => new SdkServerException($"status inesperado {(int)response.StatusCode}: {detail}", response.StatusCode),
        };
    }

    private static ProblemDetailsPayload? TryReadProblem(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ProblemDetailsPayload>(body, JsonOptions);
        }
        catch (JsonException)
        {
            // Corpo que nao e JSON e normal em erro de gateway (HTML de proxy, texto puro).
            return null;
        }
    }

    private sealed record ProblemDetailsPayload(
        string? Title,
        string? Detail,
        Dictionary<string, string[]>? Errors);
}
