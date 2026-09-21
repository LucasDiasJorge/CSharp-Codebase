using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace ApiGatewayAggregationDemo.Gateway;

/// <summary>
/// Resultado de uma chamada a um serviço de trás. O agregador precisa distinguir
/// "respondeu", "demorou demais" e "falhou" — tratar os três como a mesma coisa impede
/// tanto o diagnóstico quanto a degradação controlada.
/// </summary>
public sealed record FragmentResult(string Service, bool Succeeded, string? Error, long ElapsedMs, JsonElement? Data);

/// <summary>
/// O agregador. Chama os serviços em paralelo, aplica timeout individual e compõe uma
/// resposta única, informando o que faltou.
/// </summary>
public sealed class DashboardAggregator
{
    /// <summary>
    /// Timeout por chamada. Precisa ser menor que o do cliente do gateway, senão o
    /// cliente desiste antes e o trabalho do gateway é jogado fora.
    /// </summary>
    public static readonly TimeSpan PerServiceTimeout = TimeSpan.FromMilliseconds(1200);

    public const string CorrelationHeader = "X-Correlation-Id";

    private readonly HttpClient _http;
    private readonly ILogger<DashboardAggregator> _logger;

    public DashboardAggregator(HttpClient http, ILogger<DashboardAggregator> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Agrega em PARALELO. Sequencial, o custo seria a soma das latências; em paralelo,
    /// é a do serviço mais lento. Com 120, 400 e 250ms, a diferença é 770 contra ~400.
    /// </summary>
    public async Task<AggregationResult> AggregateAsync(string customerId, string correlationId, bool parallel, CancellationToken cancellationToken)
    {
        Stopwatch watch = Stopwatch.StartNew();

        string[] services = ["profile", "orders", "recommendations"];
        List<FragmentResult> fragments = new List<FragmentResult>();

        if (parallel)
        {
            Task<FragmentResult>[] calls = new Task<FragmentResult>[services.Length];
            for (int index = 0; index < services.Length; index++)
            {
                calls[index] = CallAsync(services[index], customerId, correlationId, cancellationToken);
            }

            fragments.AddRange(await Task.WhenAll(calls).ConfigureAwait(false));
        }
        else
        {
            // Sequencial, so para comparacao: cada chamada espera a anterior terminar.
            foreach (string service in services)
            {
                fragments.Add(await CallAsync(service, customerId, correlationId, cancellationToken).ConfigureAwait(false));
            }
        }

        watch.Stop();

        return new AggregationResult(correlationId, customerId, fragments, watch.ElapsedMilliseconds, parallel);
    }

    private async Task<FragmentResult> CallAsync(string service, string customerId, string correlationId, CancellationToken cancellationToken)
    {
        Stopwatch watch = Stopwatch.StartNew();

        // Timeout POR SERVICO, ligado ao cancelamento do cliente. Sem o linked token,
        // o gateway continuaria trabalhando depois de o cliente desistir.
        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(PerServiceTimeout);

        try
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/services/{service}/{customerId}");

            // Propaga o correlation id: e o que permite juntar, no log, as quatro
            // requisicoes (gateway + tres servicos) que atenderam a mesma chamada.
            request.Headers.Add(CorrelationHeader, correlationId);

            using HttpResponseMessage response = await _http.SendAsync(request, timeoutSource.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                watch.Stop();
                _logger.LogWarning("[{Correlacao}] {Servico} respondeu {Status}.", correlationId, service, (int)response.StatusCode);

                return new FragmentResult(service, false, $"HTTP {(int)response.StatusCode}", watch.ElapsedMilliseconds, null);
            }

            JsonElement data = await response.Content.ReadFromJsonAsync<JsonElement>(timeoutSource.Token).ConfigureAwait(false);
            watch.Stop();

            return new FragmentResult(service, true, null, watch.ElapsedMilliseconds, data);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            watch.Stop();
            _logger.LogWarning("[{Correlacao}] {Servico} excedeu {Timeout}ms.", correlationId, service, PerServiceTimeout.TotalMilliseconds);

            return new FragmentResult(service, false, $"timeout apos {PerServiceTimeout.TotalMilliseconds}ms", watch.ElapsedMilliseconds, null);
        }
        catch (Exception ex)
        {
            watch.Stop();
            _logger.LogError("[{Correlacao}] {Servico} falhou: {Erro}", correlationId, service, ex.Message);

            return new FragmentResult(service, false, ex.Message, watch.ElapsedMilliseconds, null);
        }
    }
}

public sealed record AggregationResult(
    string CorrelationId,
    string CustomerId,
    IReadOnlyList<FragmentResult> Fragments,
    long TotalElapsedMs,
    bool Parallel)
{
    public int SucceededCount
    {
        get
        {
            int count = 0;
            foreach (FragmentResult fragment in Fragments)
            {
                if (fragment.Succeeded)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public bool IsComplete => SucceededCount == Fragments.Count;

    /// <summary>
    /// Nada respondeu: aí sim é falha do gateway. Com resposta parcial, devolver erro
    /// desperdiça o que deu certo.
    /// </summary>
    public bool IsTotalFailure => SucceededCount == 0;
}
