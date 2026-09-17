using RedisDistributedLockDemo.Locking;

namespace RedisDistributedLockDemo.Work;

/// <summary>
/// Executa trabalhadores concorrentes disputando o mesmo lock, com ou sem renovação
/// automática do lease. A comparação entre os dois modos é o cerne do exemplo.
/// </summary>
public sealed class WorkerRunner
{
    private readonly DistributedLock _lock;
    private readonly ProtectedResource _resource;
    private readonly ILogger<WorkerRunner> _logger;

    public WorkerRunner(DistributedLock distributedLock, ProtectedResource resource, ILogger<WorkerRunner> logger)
    {
        _lock = distributedLock;
        _resource = resource;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WorkerOutcome>> RunAsync(
        string resource,
        int workers,
        TimeSpan lockTtl,
        TimeSpan workDuration,
        bool autoRenew,
        TimeSpan stallFirstWorker,
        CancellationToken cancellationToken)
    {
        List<Task<WorkerOutcome>> tasks = new List<Task<WorkerOutcome>>();

        for (int index = 0; index < workers; index++)
        {
            string owner = $"worker-{index + 1}";

            // A pausa so do primeiro trabalhador reproduz o caso classico: o processo
            // trava (GC, swap, rede) DEPOIS de fazer o trabalho e ANTES de gravar. Nesse
            // intervalo o lease expira, outro assume e grava — e a escrita atrasada do
            // primeiro chega por ultimo, com um fencing token velho.
            TimeSpan stall = index == 0 ? stallFirstWorker : TimeSpan.Zero;

            tasks.Add(RunWorkerAsync(resource, owner, lockTtl, workDuration, autoRenew, stall, cancellationToken));
        }

        WorkerOutcome[] outcomes = await Task.WhenAll(tasks).ConfigureAwait(false);

        return outcomes;
    }

    private async Task<WorkerOutcome> RunWorkerAsync(
        string resource,
        string owner,
        TimeSpan lockTtl,
        TimeSpan workDuration,
        bool autoRenew,
        TimeSpan stallBeforeWrite,
        CancellationToken cancellationToken)
    {
        // Tenta por ate 15s: o objetivo e que todos acabem entrando, um apos o outro.
        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(15);
        LockHandle? handle = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            handle = await _lock.TryAcquireAsync(resource, lockTtl, owner).ConfigureAwait(false);
            if (handle is not null)
            {
                break;
            }

            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        }

        if (handle is null)
        {
            return new WorkerOutcome(owner, false, 0, false, false, "nao conseguiu adquirir dentro do prazo");
        }

        using CancellationTokenSource renewalSource = new CancellationTokenSource();
        Task renewalTask = autoRenew
            ? RenewPeriodicallyAsync(handle, renewalSource.Token)
            : Task.CompletedTask;

        _resource.Enter(owner);

        try
        {
            await Task.Delay(workDuration, cancellationToken).ConfigureAwait(false);

            if (stallBeforeWrite > TimeSpan.Zero)
            {
                // Pausa simulada entre terminar o trabalho e gravar o resultado.
                _logger.LogWarning("{Dono} travou por {Pausa}ms antes de gravar.", owner, stallBeforeWrite.TotalMilliseconds);

                await Task.Delay(stallBeforeWrite, cancellationToken).ConfigureAwait(false);
            }

            // Escreve com o fencing token recebido junto do lock.
            bool written = _resource.TryWrite(owner, handle.FencingToken, $"resultado de {owner}");

            return new WorkerOutcome(
                owner,
                true,
                handle.FencingToken,
                written,
                false,
                written ? "trabalho concluido" : "escrita rejeitada pelo fencing token");
        }
        finally
        {
            _resource.Exit(owner);

            await renewalSource.CancelAsync().ConfigureAwait(false);

            try
            {
                await renewalTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Encerramento normal do renovador.
            }

            bool released = await _lock.ReleaseAsync(handle).ConfigureAwait(false);

            if (!released)
            {
                _logger.LogWarning("{Dono} nao pode liberar: o lease ja havia expirado.", owner);
            }
        }
    }

    /// <summary>
    /// Renova o lease enquanto o trabalho continua. O intervalo é uma fração do TTL —
    /// renovar só na borda não deixa margem para uma falha de rede ou uma pausa de GC.
    /// </summary>
    private async Task RenewPeriodicallyAsync(LockHandle handle, CancellationToken cancellationToken)
    {
        TimeSpan interval = TimeSpan.FromMilliseconds(handle.Ttl.TotalMilliseconds / 3);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            bool renewed = await _lock.TryRenewAsync(handle).ConfigureAwait(false);

            if (!renewed)
            {
                // Perdeu o lock. Em codigo de producao, aqui o trabalho deveria ser
                // ABORTADO — continuar significaria operar sem exclusao mutua.
                _logger.LogError("{Dono} perdeu o lease de {Recurso} durante o trabalho.", handle.Owner, handle.Resource);

                break;
            }
        }
    }
}

public sealed record WorkerOutcome(
    string Owner,
    bool AcquiredLock,
    long FencingToken,
    bool WriteAccepted,
    bool LostLease,
    string Detail);
