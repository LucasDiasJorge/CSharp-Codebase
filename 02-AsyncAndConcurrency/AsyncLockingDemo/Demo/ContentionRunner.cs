using System.Diagnostics;
using AsyncLockingDemo.Accounts;
using AsyncLockingDemo.Models;
using Microsoft.Extensions.Logging;

namespace AsyncLockingDemo.Demo;

/// <summary>
/// Submete uma conta a depósitos concorrentes e mede o resultado. É o mesmo
/// experimento para todas as estratégias; só a conta muda.
/// </summary>
public sealed class ContentionRunner
{
    private readonly ILogger<ContentionRunner> _logger;

    public ContentionRunner(ILogger<ContentionRunner> logger)
    {
        _logger = logger;
    }

    public async Task<ScenarioResult> RunAsync(IAsyncAccount account, int operations, int amount, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cenario [{Strategy}]: {Operations} depositos concorrentes de {Amount}.", account.Strategy, operations, amount);

        ThreadPoolProbe probe = new ThreadPoolProbe();
        probe.Start();

        Stopwatch watch = Stopwatch.StartNew();

        Task[] deposits = new Task[operations];
        for (int i = 0; i < operations; i++)
        {
            // Task.Run coloca cada depósito na thread pool. Nas estratégias
            // bloqueantes é aqui que as threads começam a ficar presas.
            deposits[i] = Task.Run(() => account.DepositAsync(amount, cancellationToken), cancellationToken);
        }

        await Task.WhenAll(deposits).ConfigureAwait(false);
        watch.Stop();

        ThreadPoolSnapshot snapshot = await probe.StopAsync().ConfigureAwait(false);
        ScenarioResult result = new ScenarioResult(account.Strategy, operations * amount, account.Balance, watch.Elapsed.TotalMilliseconds, snapshot);

        if (result.IsCorrect)
        {
            _logger.LogInformation(
                "  Saldo {Final} (esperado {Esperado}) em {Elapsed:F0}ms | threads da pool {Baseline} -> {Peak} | maior intervalo do heartbeat {Gap:F0}ms.",
                result.FinalBalance,
                result.ExpectedBalance,
                result.ElapsedMs,
                snapshot.BaselineThreadCount,
                snapshot.PeakThreadCount,
                snapshot.MaxHeartbeatGapMs);
        }
        else
        {
            _logger.LogWarning(
                "  Saldo {Final} (esperado {Esperado}) em {Elapsed:F0}ms | {Perdidos} depositos perdidos por sobrescrita.",
                result.FinalBalance,
                result.ExpectedBalance,
                result.ElapsedMs,
                result.LostUpdates);
        }

        return result;
    }
}
