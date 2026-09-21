using EventSourcingBankAccountDemo.Domain;
using EventSourcingBankAccountDemo.Events;
using Microsoft.Extensions.Logging;

namespace EventSourcingBankAccountDemo.Store;

/// <summary>
/// Lançada quando o fluxo mudou entre a leitura e a gravação. É a concorrência
/// otimista do event sourcing: ninguém trava nada, mas quem chega atrasado é rejeitado.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string accountId, long expectedVersion, long actualVersion)
        : base($"Conflito em {accountId}: esperava a versao {expectedVersion}, o fluxo esta na {actualVersion}.")
    {
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public long ExpectedVersion { get; }

    public long ActualVersion { get; }
}

/// <summary>
/// Armazenamento append-only de eventos, com snapshots à parte. Em memória por ser
/// exemplo; o que importa é o contrato — só se acrescenta ao fim, nunca se altera o
/// que já foi gravado.
/// </summary>
public sealed class EventStore
{
    private readonly Dictionary<string, List<AccountEvent>> _streams = new Dictionary<string, List<AccountEvent>>();
    private readonly Dictionary<string, AccountSnapshot> _snapshots = new Dictionary<string, AccountSnapshot>();
    private readonly ILogger<EventStore> _logger;

    private int _eventsReadSinceReset;

    public EventStore(ILogger<EventStore> logger)
    {
        _logger = logger;
    }

    /// <summary>Eventos lidos do disco desde o último reset — mede o ganho do snapshot.</summary>
    public int EventsReadSinceReset => _eventsReadSinceReset;

    public void ResetReadCounter() => _eventsReadSinceReset = 0;

    /// <summary>
    /// Grava os eventos pendentes verificando a versão esperada. Se outro processo
    /// gravou nesse meio-tempo, nada é escrito e o chamador precisa reler e decidir.
    /// </summary>
    public void Append(string accountId, IReadOnlyList<AccountEvent> events, long expectedVersion)
    {
        if (events.Count == 0)
        {
            return;
        }

        if (!_streams.TryGetValue(accountId, out List<AccountEvent>? stream))
        {
            stream = new List<AccountEvent>();
            _streams[accountId] = stream;
        }

        long currentVersion = stream.Count == 0 ? 0 : stream[^1].Version;

        if (currentVersion != expectedVersion)
        {
            throw new ConcurrencyConflictException(accountId, expectedVersion, currentVersion);
        }

        stream.AddRange(events);

        _logger.LogInformation(
            "{Quantidade} evento(s) gravado(s) em {Conta}. Fluxo agora na versao {Versao}.",
            events.Count,
            accountId,
            stream[^1].Version);
    }

    /// <summary>Lê o fluxo a partir de uma versão (exclusiva).</summary>
    public IReadOnlyList<AccountEvent> ReadStream(string accountId, long fromVersionExclusive = 0)
    {
        if (!_streams.TryGetValue(accountId, out List<AccountEvent>? stream))
        {
            return [];
        }

        List<AccountEvent> slice = new List<AccountEvent>();
        foreach (AccountEvent accountEvent in stream)
        {
            if (accountEvent.Version > fromVersionExclusive)
            {
                slice.Add(accountEvent);
            }
        }

        _eventsReadSinceReset += slice.Count;

        return slice;
    }

    public long GetCurrentVersion(string accountId)
    {
        return _streams.TryGetValue(accountId, out List<AccountEvent>? stream) && stream.Count > 0
            ? stream[^1].Version
            : 0;
    }

    public void SaveSnapshot(AccountSnapshot snapshot)
    {
        _snapshots[snapshot.AccountId] = snapshot;

        _logger.LogInformation("Snapshot de {Conta} salvo na versao {Versao}.", snapshot.AccountId, snapshot.Version);
    }

    public AccountSnapshot? GetSnapshot(string accountId)
    {
        return _snapshots.TryGetValue(accountId, out AccountSnapshot? snapshot) ? snapshot : null;
    }

    /// <summary>
    /// Carrega o agregado. Com snapshot, lê apenas os eventos posteriores a ele; sem,
    /// lê o fluxo inteiro. O resultado é idêntico — a diferença é só quanto se lê.
    /// </summary>
    public BankAccount? Load(string accountId, bool useSnapshot)
    {
        AccountSnapshot? snapshot = useSnapshot ? GetSnapshot(accountId) : null;
        long from = snapshot?.Version ?? 0;

        IReadOnlyList<AccountEvent> stream = ReadStream(accountId, from);

        if (snapshot is null && stream.Count == 0)
        {
            return null;
        }

        return BankAccount.Rehydrate(stream, snapshot);
    }
}
