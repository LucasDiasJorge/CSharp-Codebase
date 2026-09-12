using System.Text;
using AsyncLockingDemo.Accounts;
using AsyncLockingDemo.Models;
using Microsoft.Extensions.Logging;

namespace AsyncLockingDemo.Demo;

/// <summary>
/// Executa os quatro cenários na ordem em que a lição se constrói: reproduzir a
/// corrida, corrigi-la bloqueando, mostrar que o bloqueio é o problema e então
/// trocar a espera bloqueante por <c>WaitAsync</c>.
/// </summary>
public sealed class AsyncLockingDemoRunner
{
    private const int Operations = 24;
    private const int DepositAmount = 1;
    private static readonly TimeSpan IoDelay = TimeSpan.FromMilliseconds(20);
    private static readonly TimeSpan ConsoleFlushDelay = TimeSpan.FromMilliseconds(200);

    private readonly ContentionRunner _contentionRunner;
    private readonly ReleasePitfallDemo _pitfallDemo;
    private readonly ILogger<AsyncLockingDemoRunner> _logger;

    public AsyncLockingDemoRunner(ContentionRunner contentionRunner, ReleasePitfallDemo pitfallDemo, ILogger<AsyncLockingDemoRunner> logger)
    {
        _contentionRunner = contentionRunner;
        _pitfallDemo = pitfallDemo;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processadores: {Cpus}. Cada deposito le o saldo, espera {Delay}ms de I/O simulado e escreve de volta.",
            Environment.ProcessorCount,
            IoDelay.TotalMilliseconds);

        List<ScenarioResult> results = new List<ScenarioResult>();

        UnsafeAccount unsafeAccount = new UnsafeAccount(IoDelay);
        results.Add(await _contentionRunner.RunAsync(unsafeAccount, Operations, DepositAmount, cancellationToken).ConfigureAwait(false));

        BlockingLockAccount lockAccount = new BlockingLockAccount(IoDelay);
        results.Add(await _contentionRunner.RunAsync(lockAccount, Operations, DepositAmount, cancellationToken).ConfigureAwait(false));

        // A pool nao encolhe entre os cenarios: as threads injetadas no cenario 2
        // continuam vivas aqui. Por isso os cenarios bloqueantes devem ser comparados
        // pelas colunas de thread e de heartbeat, nao pelo tempo total de um contra o outro.
        _logger.LogInformation("As threads injetadas no cenario anterior continuam na pool; o proximo cenario ja comeca com ela aquecida.");

        using BlockingSemaphoreAccount blockingSemaphoreAccount = new BlockingSemaphoreAccount(IoDelay);
        results.Add(await _contentionRunner.RunAsync(blockingSemaphoreAccount, Operations, DepositAmount, cancellationToken).ConfigureAwait(false));

        using SemaphoreAccount semaphoreAccount = new SemaphoreAccount(IoDelay);
        results.Add(await _contentionRunner.RunAsync(semaphoreAccount, Operations, DepositAmount, cancellationToken).ConfigureAwait(false));

        // O provider de console do logging grava numa fila propria; sem esta pausa a
        // tabela sai no meio das linhas do ultimo cenario.
        await Task.Delay(ConsoleFlushDelay, cancellationToken).ConfigureAwait(false);
        WriteSummary(results);

        await _pitfallDemo.RunForgottenReleaseAsync(cancellationToken).ConfigureAwait(false);
        await _pitfallDemo.RunReentrancyAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// A tabela final e apresentacao, nao evento de log: vai direto para o console
    /// para preservar o alinhamento das colunas, que o prefixo do logger quebraria.
    /// </summary>
    private static void WriteSummary(IReadOnlyList<ScenarioResult> results)
    {
        StringBuilder table = new StringBuilder();
        table.AppendLine();
        table.AppendLine("Resumo dos cenarios");
        table.AppendLine("Estrategia                   Saldo  Correto  Tempo(ms)  Threads  Heartbeat max(ms)");

        foreach (ScenarioResult result in results)
        {
            table.AppendLine(string.Format(
                "{0,-26}  {1,5}  {2,-7}  {3,9:F0}  {4,3}->{5,-3}  {6,17:F0}",
                result.Strategy,
                result.FinalBalance,
                result.IsCorrect ? "sim" : "NAO",
                result.ElapsedMs,
                result.ThreadPool.BaselineThreadCount,
                result.ThreadPool.PeakThreadCount,
                result.ThreadPool.MaxHeartbeatGapMs));
        }

        Console.WriteLine(table.ToString());
    }
}
