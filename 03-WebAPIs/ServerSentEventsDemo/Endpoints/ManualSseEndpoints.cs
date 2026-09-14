using System.Text.Json;
using ServerSentEventsDemo.Events;

namespace ServerSentEventsDemo.Endpoints;

/// <summary>
/// O mesmo fluxo escrito à mão, byte a byte. Existe para mostrar o que
/// <c>TypedResults.ServerSentEvents</c> faz por baixo: um formato de texto com quatro
/// campos e linha em branco separando eventos.
/// </summary>
public static class ManualSseEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public static void MapManualSseEndpoints(this WebApplication app)
    {
        app.MapGet("/stream/raw", async (HttpContext context, PriceTickFeed feed, ILoggerFactory loggerFactory) =>
        {
            ILogger logger = loggerFactory.CreateLogger("ManualSse");

            // Os tres headers que definem um fluxo SSE.
            context.Response.Headers.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";

            // Desliga o buffer de proxies reversos (nginx). Sem isso o fluxo chega em
            // blocos, ou nao chega, e o bug parece ser da aplicacao.
            context.Response.Headers["X-Accel-Buffering"] = "no";

            long lastEventId = LastEventIdReader.Read(context.Request);

            // retry: vale para a conexao inteira e pode ser enviado sozinho.
            await context.Response.WriteAsync("retry: 2000\n\n", context.RequestAborted);
            await context.Response.Body.FlushAsync(context.RequestAborted);

            try
            {
                await foreach (PriceTick tick in feed.SubscribeAsync(lastEventId, context.RequestAborted))
                {
                    string payload = JsonSerializer.Serialize(tick, JsonOptions);

                    // Um evento: id, tipo, dados e a linha em branco que o encerra.
                    // Esquecer a linha em branco faz o cliente esperar para sempre.
                    await context.Response.WriteAsync($"id: {tick.Id}\n", context.RequestAborted);
                    await context.Response.WriteAsync("event: price\n", context.RequestAborted);
                    await context.Response.WriteAsync($"data: {payload}\n\n", context.RequestAborted);

                    // Sem flush explicito a resposta fica no buffer e o cliente nao ve
                    // nada ate o buffer encher.
                    await context.Response.Body.FlushAsync(context.RequestAborted);
                }
            }
            catch (OperationCanceledException)
            {
                // Desconexao de cliente e o fim normal de um fluxo SSE, nao um erro.
                logger.LogInformation("Fluxo manual encerrado: o cliente desconectou.");
            }
        });

        app.MapGet("/connections", (PriceTickFeed feed) => Results.Ok(new
        {
            activeConnections = feed.ActiveConnections,
            lastEventId = feed.LastEventId
        }));
    }
}
