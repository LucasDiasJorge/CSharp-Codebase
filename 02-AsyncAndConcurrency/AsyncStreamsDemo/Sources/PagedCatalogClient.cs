using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace AsyncStreamsDemo.Sources;

/// <summary>
/// Cliente de uma API paginada exposta como um unico stream de itens.
///
/// O consumidor nao ve paginas: ele itera itens. E, como o iterador so avanca quando alguem
/// pede o proximo item, a pagina seguinte so e buscada se realmente for necessaria — sair do
/// laco com `break` evita as chamadas restantes.
/// </summary>
public sealed class PagedCatalogClient
{
    private readonly ILogger<PagedCatalogClient> logger;
    private readonly TimeSpan pageLatency;
    private readonly int pageSize;
    private readonly int totalPages;

    private int fetchedPageCount;

    public PagedCatalogClient(ILogger<PagedCatalogClient> logger, TimeSpan pageLatency, int pageSize, int totalPages)
    {
        this.logger = logger;
        this.pageLatency = pageLatency;
        this.pageSize = pageSize;
        this.totalPages = totalPages;
    }

    /// <summary>Quantas paginas foram realmente buscadas desde o ultimo <see cref="ResetCounters"/>.</summary>
    public int FetchedPageCount => Volatile.Read(ref fetchedPageCount);

    public void ResetCounters()
    {
        Volatile.Write(ref fetchedPageCount, 0);
    }

    public async IAsyncEnumerable<string> StreamItemsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int page = 1; page <= totalPages; page++)
        {
            // A chamada so acontece quando o consumidor pede o primeiro item desta pagina.
            await Task.Delay(pageLatency, cancellationToken);
            Interlocked.Increment(ref fetchedPageCount);
            logger.LogInformation("[api] pagina {Page} de {Total} buscada", page, totalPages);

            for (int position = 1; position <= pageSize; position++)
            {
                yield return $"item-{((page - 1) * pageSize) + position:D2}";
            }
        }
    }
}
