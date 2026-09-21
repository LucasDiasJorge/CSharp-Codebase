using EventSourcingBankAccountDemo.Domain;
using EventSourcingBankAccountDemo.Events;
using EventSourcingBankAccountDemo.Store;
using Microsoft.Extensions.Logging;

namespace EventSourcingBankAccountDemo.Demo;

/// <summary>
/// Cinco cenários: reconstruir estado a partir de eventos, auditoria gratuita,
/// concorrência otimista, snapshot como atalho e a viagem no tempo.
/// </summary>
public sealed class EventSourcingDemoRunner
{
    private readonly EventStore _store;
    private readonly ILogger<EventSourcingDemoRunner> _logger;

    public EventSourcingDemoRunner(EventStore store, ILogger<EventSourcingDemoRunner> logger)
    {
        _store = store;
        _logger = logger;
    }

    public void RunAll()
    {
        RunRebuildFromEvents();
        RunAuditTrail();
        RunOptimisticConcurrency();
        RunSnapshot();
        RunTimeTravel();
    }

    private void RunRebuildFromEvents()
    {
        Section("1. O estado e derivado dos eventos, nao armazenado");

        BankAccount account = BankAccount.Open("conta-1", "Ana", 1000m);
        account.Deposit(500m);
        account.Withdraw(200m);

        Persist(account);

        _logger.LogInformation("Em memoria: saldo {Saldo:F2}, versao {Versao}.", account.Balance, account.Version);

        // Recarrega do zero: nenhuma coluna de saldo foi gravada, so os eventos.
        BankAccount reloaded = _store.Load("conta-1", useSnapshot: false)!;

        _logger.LogInformation(
            "Recarregado do fluxo: saldo {Saldo:F2}, versao {Versao}. Mesmo estado, reconstruido evento a evento.",
            reloaded.Balance,
            reloaded.Version);
    }

    private void RunAuditTrail()
    {
        Section("2. Auditoria sai de graca: o fluxo E o historico");

        foreach (AccountEvent accountEvent in _store.ReadStream("conta-1"))
        {
            _logger.LogInformation("  v{Versao}: {Descricao}", accountEvent.Version, accountEvent.Describe());
        }

        _logger.LogInformation(
            "Nenhuma tabela de log foi escrita. Em um modelo com coluna de saldo, esse historico precisaria ser mantido a parte — e poderia divergir.");
    }

    private void RunOptimisticConcurrency()
    {
        Section("3. Concorrencia otimista: dois processos, o mesmo agregado");

        // Dois carregamentos independentes, ambos na mesma versao.
        BankAccount processA = _store.Load("conta-1", useSnapshot: false)!;
        BankAccount processB = _store.Load("conta-1", useSnapshot: false)!;

        long sharedVersion = processA.Version;
        _logger.LogInformation("A e B carregaram a conta na versao {Versao}, saldo {Saldo:F2}.", sharedVersion, processA.Balance);

        processA.Withdraw(300m);
        Persist(processA);
        _logger.LogInformation("A sacou 300 e gravou. Fluxo na versao {Versao}.", _store.GetCurrentVersion("conta-1"));

        processB.Withdraw(1200m);

        try
        {
            Persist(processB);
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning("B foi rejeitado: {Erro}", ex.Message);
            _logger.LogInformation(
                "Sem essa checagem, B teria gravado sobre um estado que ja nao existia — e o saldo ficaria errado sem erro nenhum.");
        }

        // O caminho correto apos o conflito: reler e decidir de novo.
        BankAccount retried = _store.Load("conta-1", useSnapshot: false)!;
        _logger.LogInformation("B releu: saldo agora e {Saldo:F2}.", retried.Balance);

        try
        {
            retried.Withdraw(1200m);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Na releitura a regra de negocio barra: {Erro}", ex.Message);
        }
    }

    private void RunSnapshot()
    {
        Section("4. Snapshot: atalho de leitura, nao fonte da verdade");

        BankAccount busy = BankAccount.Open("conta-2", "Bruno", 100m);
        Persist(busy);

        for (int index = 0; index < 200; index++)
        {
            busy.Deposit(10m);
        }

        Persist(busy);

        _store.ResetReadCounter();
        BankAccount withoutSnapshot = _store.Load("conta-2", useSnapshot: false)!;
        int readsWithout = _store.EventsReadSinceReset;

        _store.SaveSnapshot(withoutSnapshot.TakeSnapshot());

        // Mais movimento depois do snapshot.
        withoutSnapshot.Deposit(55m);
        Persist(withoutSnapshot);

        _store.ResetReadCounter();
        BankAccount withSnapshot = _store.Load("conta-2", useSnapshot: true)!;
        int readsWith = _store.EventsReadSinceReset;

        _store.ResetReadCounter();
        BankAccount fullReplay = _store.Load("conta-2", useSnapshot: false)!;
        int readsFull = _store.EventsReadSinceReset;

        _logger.LogInformation(
            "Sem snapshot: {SemSnapshot} eventos lidos. Com snapshot: {ComSnapshot}. Releitura completa: {Completa}.",
            readsWithout,
            readsWith,
            readsFull);

        _logger.LogInformation(
            "Saldo com snapshot {ComSaldo:F2} e por releitura completa {CompletoSaldo:F2} — identicos, como tem de ser.",
            withSnapshot.Balance,
            fullReplay.Balance);
    }

    private void RunTimeTravel()
    {
        Section("5. Estado em qualquer ponto do passado");

        IReadOnlyList<AccountEvent> full = _store.ReadStream("conta-1");

        foreach (long version in new long[] { 1, 3, full.Count })
        {
            List<AccountEvent> upTo = new List<AccountEvent>();
            foreach (AccountEvent accountEvent in full)
            {
                if (accountEvent.Version <= version)
                {
                    upTo.Add(accountEvent);
                }
            }

            BankAccount atVersion = BankAccount.Rehydrate(upTo);
            _logger.LogInformation("  na versao {Versao}: saldo {Saldo:F2}", version, atVersion.Balance);
        }

        _logger.LogInformation("Reproduzir o passado e so parar de aplicar eventos mais cedo.");
    }

    /// <summary>Grava os pendentes usando a versão anterior como expectativa.</summary>
    private void Persist(BankAccount account)
    {
        long expected = account.Version - account.UncommittedEvents.Count;

        _store.Append(account.AccountId, account.UncommittedEvents, expected);
        account.MarkCommitted();
    }

    /// <summary>
    /// Título de seção. A pausa existe porque o provider de console do logging grava em
    /// fila própria: sem ela, o cabeçalho escrito direto no console aparece antes das
    /// mensagens do cenário anterior.
    /// </summary>
    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
