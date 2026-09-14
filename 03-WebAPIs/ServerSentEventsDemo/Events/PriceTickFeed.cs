using System.Runtime.CompilerServices;

namespace ServerSentEventsDemo.Events;

/// <summary>
/// Fonte única de eventos, compartilhada por todos os clientes conectados. Existe um
/// gerador central, e não um por conexão, porque é isso que dá sentido ao
/// <c>Last-Event-ID</c>: o id precisa significar a mesma coisa para todo mundo.
/// </summary>
public sealed class PriceTickFeed : BackgroundService
{
    private const int HistoryLimit = 50;
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(1);
    private static readonly string[] Symbols = ["PETR4", "VALE3", "ITUB4"];

    private readonly object _gate = new object();
    private readonly List<PriceTick> _history = new List<PriceTick>();
    private readonly Random _random = new Random(20260913);
    private readonly ILogger<PriceTickFeed> _logger;

    // Sinal de "chegou evento novo". Cada tick completa a instância atual e coloca
    // outra no lugar; quem espera acorda e volta a ler o histórico.
    private TaskCompletionSource _tickSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

    private long _nextId = 1;
    private int _activeConnections;

    public PriceTickFeed(ILogger<PriceTickFeed> logger)
    {
        _logger = logger;
    }

    public int ActiveConnections => Volatile.Read(ref _activeConnections);

    public long LastEventId
    {
        get
        {
            lock (_gate)
            {
                return _nextId - 1;
            }
        }
    }

    /// <summary>
    /// Entrega tudo o que houver depois de <paramref name="lastEventId"/> e então
    /// acompanha o fluxo ao vivo. O cancelamento vem do
    /// <c>HttpContext.RequestAborted</c>: quando o cliente some, o loop termina.
    /// </summary>
    public async IAsyncEnumerable<PriceTick> SubscribeAsync(
        long lastEventId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int connectionNumber = Interlocked.Increment(ref _activeConnections);
        _logger.LogInformation(
            "Cliente conectado a partir do evento {UltimoId}. Conexoes ativas: {Ativas}.",
            lastEventId,
            connectionNumber);

        long delivered = 0;
        long cursor = lastEventId;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Capturar o sinal ANTES de ler o histórico. Na ordem inversa, um tick
                // publicado entre a leitura e a espera seria perdido.
                Task waitForNext = Volatile.Read(ref _tickSignal).Task;

                IReadOnlyList<PriceTick> pending = GetSince(cursor);
                foreach (PriceTick tick in pending)
                {
                    cursor = tick.Id;
                    delivered++;
                    yield return tick;
                }

                await waitForNext.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            int remaining = Interlocked.Decrement(ref _activeConnections);
            _logger.LogInformation(
                "Cliente desconectou apos {Entregues} eventos. Conexoes ativas: {Ativas}.",
                delivered,
                remaining);
        }
    }

    /// <summary>Histórico retido, usado para responder a uma reconexão.</summary>
    public IReadOnlyList<PriceTick> GetSince(long lastEventId)
    {
        lock (_gate)
        {
            List<PriceTick> pending = new List<PriceTick>();
            foreach (PriceTick tick in _history)
            {
                if (tick.Id > lastEventId)
                {
                    pending.Add(tick);
                }
            }

            return pending;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TickInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            Publish();
        }
    }

    private void Publish()
    {
        TaskCompletionSource previousSignal;

        lock (_gate)
        {
            string symbol = Symbols[_random.Next(Symbols.Length)];
            decimal price = Math.Round(20m + (decimal)(_random.NextDouble() * 10d), 2);

            _history.Add(new PriceTick(_nextId, symbol, price, DateTimeOffset.UtcNow));
            _nextId++;

            // Histórico limitado: uma reconexão muito atrasada perde eventos, e é assim
            // que funciona em qualquer servidor real. O cliente precisa tolerar buraco.
            if (_history.Count > HistoryLimit)
            {
                _history.RemoveAt(0);
            }

            previousSignal = _tickSignal;
            _tickSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        previousSignal.SetResult();
    }
}
