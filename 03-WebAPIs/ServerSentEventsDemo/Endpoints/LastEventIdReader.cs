namespace ServerSentEventsDemo.Endpoints;

/// <summary>
/// Lê o ponto de retomada da requisição. O navegador envia o header
/// <c>Last-Event-ID</c> automaticamente ao reconectar um <c>EventSource</c>; a query
/// string existe só para dar como testar o mesmo comportamento por curl.
/// </summary>
public static class LastEventIdReader
{
    public static long Read(HttpRequest request)
    {
        if (request.Headers.TryGetValue("Last-Event-ID", out Microsoft.Extensions.Primitives.StringValues header)
            && long.TryParse(header.ToString(), out long fromHeader))
        {
            return fromHeader;
        }

        if (request.Query.TryGetValue("lastEventId", out Microsoft.Extensions.Primitives.StringValues query)
            && long.TryParse(query.ToString(), out long fromQuery))
        {
            return fromQuery;
        }

        // Zero significa "manda tudo o que voce ainda tiver".
        return 0;
    }
}
