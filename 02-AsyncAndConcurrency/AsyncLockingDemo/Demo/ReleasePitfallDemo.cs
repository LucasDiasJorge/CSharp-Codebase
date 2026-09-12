using Microsoft.Extensions.Logging;

namespace AsyncLockingDemo.Demo;

/// <summary>
/// As duas armadilhas que aparecem depois que o SemaphoreSlim entra no código:
/// esquecer o <c>Release</c> e supor que ele é reentrante como o <c>lock</c>.
/// </summary>
public sealed class ReleasePitfallDemo
{
    private static readonly TimeSpan AcquireTimeout = TimeSpan.FromMilliseconds(300);

    private readonly ILogger<ReleasePitfallDemo> _logger;

    public ReleasePitfallDemo(ILogger<ReleasePitfallDemo> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Uma exceção na seção crítica sem <c>finally</c> fecha o semáforo para sempre.
    /// O timeout do <c>WaitAsync</c> é o que transforma o deadlock em diagnóstico.
    /// </summary>
    public async Task RunForgottenReleaseAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Armadilha 1: Release esquecido apos excecao.");

        using SemaphoreSlim gate = new SemaphoreSlim(1, 1);

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            throw new InvalidOperationException("falha simulada dentro da secao critica");
        }
        catch (InvalidOperationException ex)
        {
            // Sem finally: o catch trata a exceção e a execução segue, mas a vaga
            // do semáforo continua ocupada por um dono que não existe mais.
            _logger.LogWarning("  Excecao tratada: {Mensagem}. Nenhum Release foi executado.", ex.Message);
        }

        bool reacquired = await gate.WaitAsync(AcquireTimeout, cancellationToken).ConfigureAwait(false);
        _logger.LogWarning(
            "  Nova tentativa de entrar apos {Timeout}ms: {Resultado}. Em producao isso seria um travamento silencioso, nao um erro.",
            AcquireTimeout.TotalMilliseconds,
            reacquired ? "conseguiu" : "negada");

        if (reacquired)
        {
            gate.Release();
        }
    }

    /// <summary>
    /// <c>lock</c> é reentrante por thread; <see cref="SemaphoreSlim"/> conta vagas e
    /// não sabe quem já entrou. Chamar um método protegido de dentro de outro método
    /// protegido pelo mesmo semáforo trava.
    /// </summary>
    public async Task RunReentrancyAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Armadilha 2: reentrancia.");

        object monitorGate = new object();
        lock (monitorGate)
        {
            lock (monitorGate)
            {
                _logger.LogInformation("  lock aninhado na mesma thread: entrou (Monitor e reentrante).");
            }
        }

        using SemaphoreSlim gate = new SemaphoreSlim(1, 1);

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            bool reentered = await gate.WaitAsync(AcquireTimeout, cancellationToken).ConfigureAwait(false);
            _logger.LogWarning(
                "  SemaphoreSlim aninhado no mesmo fluxo apos {Timeout}ms: {Resultado}. Sem o timeout, seria deadlock.",
                AcquireTimeout.TotalMilliseconds,
                reentered ? "entrou" : "negado");

            if (reentered)
            {
                gate.Release();
            }
        }
        finally
        {
            gate.Release();
        }
    }
}
