using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using ServerSentEventsDemo.Events;

namespace ServerSentEventsDemo.Endpoints;

/// <summary>
/// Fluxo escrito com o suporte nativo do ASP.NET Core 10
/// (<c>TypedResults.ServerSentEvents</c>): o framework cuida dos headers, do formato
/// do protocolo e do flush a cada item.
/// </summary>
public static class NativeSseEndpoints
{
    private static readonly TimeSpan ReconnectionInterval = TimeSpan.FromSeconds(2);

    public static void MapNativeSseEndpoints(this WebApplication app)
    {
        app.MapGet("/stream/prices", (HttpContext context, PriceTickFeed feed) =>
        {
            long lastEventId = LastEventIdReader.Read(context.Request);

            // RequestAborted é o que encerra o fluxo quando o cliente desconecta.
            // Sem repassá-lo, o servidor continua gerando eventos para ninguém.
            IAsyncEnumerable<SseItem<PriceTick>> items =
                ToSseItems(feed.SubscribeAsync(lastEventId, context.RequestAborted), context.RequestAborted);

            // Sem o argumento eventType: a sobrecarga que recebe SseItem<T> nao tem esse
            // parametro, porque cada item ja carrega o proprio tipo. Passar eventType
            // aqui casa com a sobrecarga generica, que serializaria o SseItem inteiro
            // como payload — compila, responde 200 e entrega lixo.
            return TypedResults.ServerSentEvents(items);
        });

        // Fluxo que termina sozinho. SSE nao tem "fim de stream" no protocolo: quando o
        // servidor fecha, o navegador reconecta. Por isso o ultimo evento e um "done"
        // combinado com o cliente, que chama EventSource.close() ao recebe-lo.
        app.MapGet("/stream/finite", (HttpContext context, PriceTickFeed feed, int count = 5) =>
        {
            IAsyncEnumerable<SseItem<PriceTick>> items =
                TakeThenComplete(feed.SubscribeAsync(feed.LastEventId, context.RequestAborted), count, context.RequestAborted);

            return TypedResults.ServerSentEvents(items);
        });
    }

    private static async IAsyncEnumerable<SseItem<PriceTick>> ToSseItems(
        IAsyncEnumerable<PriceTick> ticks,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (PriceTick tick in ticks.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            // EventId vira o campo id: — e volta no Last-Event-ID da proxima conexao.
            // ReconnectionInterval vira retry:, que instrui o navegador sobre quanto
            // esperar antes de tentar de novo.
            yield return new SseItem<PriceTick>(tick, "price")
            {
                EventId = tick.Id.ToString(),
                ReconnectionInterval = ReconnectionInterval
            };
        }
    }

    private static async IAsyncEnumerable<SseItem<PriceTick>> TakeThenComplete(
        IAsyncEnumerable<PriceTick> ticks,
        int count,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int sent = 0;
        PriceTick? last = null;

        await foreach (PriceTick tick in ticks.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            last = tick;
            sent++;

            yield return new SseItem<PriceTick>(tick, "price")
            {
                EventId = tick.Id.ToString()
            };

            if (sent >= count)
            {
                break;
            }
        }

        if (last is not null)
        {
            // Tipo de evento combinado com o cliente para dizer "acabou de verdade".
            yield return new SseItem<PriceTick>(last, "done");
        }
    }
}
