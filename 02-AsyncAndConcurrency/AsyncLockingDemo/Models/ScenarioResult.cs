namespace AsyncLockingDemo.Models;

/// <summary>
/// Resultado de um cenário de contenção: se o saldo final bateu com o esperado e
/// quanto custou chegar lá.
/// </summary>
public sealed class ScenarioResult
{
    public ScenarioResult(string strategy, int expectedBalance, int finalBalance, double elapsedMs, ThreadPoolSnapshot threadPool)
    {
        Strategy = strategy;
        ExpectedBalance = expectedBalance;
        FinalBalance = finalBalance;
        ElapsedMs = elapsedMs;
        ThreadPool = threadPool;
    }

    public string Strategy { get; }

    public int ExpectedBalance { get; }

    public int FinalBalance { get; }

    public double ElapsedMs { get; }

    public ThreadPoolSnapshot ThreadPool { get; }

    /// <summary>Depósitos que se perderam por sobrescrita.</summary>
    public int LostUpdates => ExpectedBalance - FinalBalance;

    public bool IsCorrect => FinalBalance == ExpectedBalance;
}
