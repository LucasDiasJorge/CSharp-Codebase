using Microsoft.Extensions.Logging;
using ObserverStockAlertsDemo.Events;
using ObserverStockAlertsDemo.Observer;

namespace ObserverStockAlertsDemo.Demo;

/// <summary>
/// Seis cenários: o padrão à mão, o equivalente com <c>event</c>, e as quatro
/// armadilhas que aparecem em código real.
/// </summary>
public sealed class ObserverDemoRunner
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ObserverDemoRunner> _logger;

    public ObserverDemoRunner(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ObserverDemoRunner>();
    }

    public void RunAll()
    {
        RunManualObserver();
        RunEventBased();
        RunFailurePropagation();
        RunLambdaUnsubscribe();
        RunLapsedListener();
    }

    private void RunManualObserver()
    {
        Section("1. Observer escrito a mao: inscrever, notificar, remover");

        StockTicker ticker = new StockTicker(_loggerFactory.CreateLogger<StockTicker>(), isolateFailures: true);

        ThresholdAlertObserver alert = new ThresholdAlertObserver("alerta-100", 100m, _logger);
        AuditObserver audit = new AuditObserver("auditoria", _logger);

        ticker.Attach(alert);
        ticker.Attach(audit);

        ticker.Publish(new StockQuote("PETR4", 95m, DateTimeOffset.UtcNow));
        ticker.Publish(new StockQuote("PETR4", 104m, DateTimeOffset.UtcNow));

        ticker.Detach(alert);
        ticker.Publish(new StockQuote("PETR4", 120m, DateTimeOffset.UtcNow));

        _logger.LogInformation(
            "Resultado: alerta disparou {Alertas} vez(es) e a auditoria viu {Vistas} cotacoes — o alerta perdeu a ultima porque saiu antes.",
            alert.AlertsFired,
            audit.SeenCount);
    }

    private void RunEventBased()
    {
        Section("2. O mesmo com event do C#: o padrao ja vem na linguagem");

        EventBasedTicker ticker = new EventBasedTicker(_loggerFactory.CreateLogger<EventBasedTicker>());

        // Guardar o handler em uma variavel e o que torna o -= possivel mais adiante.
        EventHandler<StockQuote> alertHandler = (_, quote) =>
        {
            if (quote.Price >= 100m)
            {
                _logger.LogInformation("  [alerta-evento] ALERTA: {Simbolo} a {Preco:F2}.", quote.Symbol, quote.Price);
            }
        };

        EventHandler<StockQuote> auditHandler = (_, quote) =>
            _logger.LogInformation("  [auditoria-evento] registrou {Simbolo} = {Preco:F2}.", quote.Symbol, quote.Price);

        ticker.QuotePublished += alertHandler;
        ticker.QuotePublished += auditHandler;

        ticker.Publish(new StockQuote("VALE3", 104m, DateTimeOffset.UtcNow));

        ticker.QuotePublished -= alertHandler;
        _logger.LogInformation("Removido o handler de alerta. Assinantes: {Total}.", ticker.SubscriberCount);

        ticker.Publish(new StockQuote("VALE3", 130m, DateTimeOffset.UtcNow));
    }

    private void RunFailurePropagation()
    {
        Section("3. Um observador que falha: quem decide o estrago e o subject");

        StockTicker naive = new StockTicker(_loggerFactory.CreateLogger<StockTicker>(), isolateFailures: false);
        AuditObserver afterFaulty = new AuditObserver("auditoria-depois", _logger);

        naive.Attach(new FaultyObserver("observador-quebrado"));
        naive.Attach(afterFaulty);

        try
        {
            naive.Publish(new StockQuote("ITUB4", 30m, DateTimeOffset.UtcNow));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Publicacao abortou: {Erro}", ex.Message);
        }

        _logger.LogInformation("SEM isolamento, a auditoria recebeu {Quantidade} cotacao(oes).", afterFaulty.SeenCount);

        StockTicker isolated = new StockTicker(_loggerFactory.CreateLogger<StockTicker>(), isolateFailures: true);
        AuditObserver protectedAudit = new AuditObserver("auditoria-protegida", _logger);

        isolated.Attach(new FaultyObserver("observador-quebrado"));
        isolated.Attach(protectedAudit);
        isolated.Publish(new StockQuote("ITUB4", 31m, DateTimeOffset.UtcNow));

        _logger.LogInformation("COM isolamento, a auditoria recebeu {Quantidade} cotacao(oes).", protectedAudit.SeenCount);
    }

    private void RunLambdaUnsubscribe()
    {
        Section("4. Nao da para remover um lambda que voce nao guardou");

        EventBasedTicker ticker = new EventBasedTicker(_loggerFactory.CreateLogger<EventBasedTicker>());

        ticker.QuotePublished += (_, quote) => _logger.LogInformation("  [lambda-anonimo] viu {Simbolo}.", quote.Symbol);
        _logger.LogInformation("Inscrito com lambda anonimo. Assinantes: {Total}.", ticker.SubscriberCount);

        // Um lambda com o MESMO corpo e um objeto diferente. O -= nao encontra nada
        // para remover, e falha em silencio — sem erro, sem aviso.
        ticker.QuotePublished -= (_, quote) => _logger.LogInformation("  [lambda-anonimo] viu {Simbolo}.", quote.Symbol);

        _logger.LogWarning(
            "Depois do -= com outro lambda de corpo identico: {Total} assinante(s). Nada foi removido, e nenhum erro apareceu.",
            ticker.SubscriberCount);
    }

    private void RunLapsedListener()
    {
        Section("5. Lapsed listener: observador que nao se desinscreve nao e coletado");

        StockTicker leaky = new StockTicker(_loggerFactory.CreateLogger<StockTicker>(), isolateFailures: true);
        StockTicker clean = new StockTicker(_loggerFactory.CreateLogger<StockTicker>(), isolateFailures: true);

        WeakReference leakedReference = AttachAndForget(leaky, "observador-vazado", detach: false);
        WeakReference cleanReference = AttachAndForget(clean, "observador-removido", detach: true);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        _logger.LogWarning(
            "Apos coleta: o observador que NAO se desinscreveu continua vivo? {Vazado}. O que se desinscreveu? {Limpo}.",
            leakedReference.IsAlive,
            cleanReference.IsAlive);

        _logger.LogInformation(
            "O ticker com vazamento ainda lista {Vazado} observador(es); o outro, {Limpo}.",
            leaky.ObserverCount,
            clean.ObserverCount);
    }

    /// <summary>
    /// Inscreve um observador pesado em um escopo separado e devolve só uma referência
    /// fraca. Depois deste método não há mais nenhuma referência forte local — se o
    /// objeto continuar vivo, é porque o subject o está segurando.
    /// </summary>
    private static WeakReference AttachAndForget(StockTicker ticker, string name, bool detach)
    {
        HeavyObserver observer = new HeavyObserver(name, 1024 * 512);
        ticker.Attach(observer);

        if (detach)
        {
            ticker.Detach(observer);
        }

        return new WeakReference(observer);
    }

    private void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
