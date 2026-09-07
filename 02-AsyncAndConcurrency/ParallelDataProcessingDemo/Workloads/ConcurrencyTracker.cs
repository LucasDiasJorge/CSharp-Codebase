namespace ParallelDataProcessingDemo.Workloads;

/// <summary>
/// Mede quantas execucoes coexistem de fato. E este numero, e nao a estrategia escolhida,
/// que diz se houve paralelismo: uma carga de CPU sem offload registra pico 1 mesmo dentro
/// de um <c>Task.WhenAll</c>.
///
/// Os contadores sao atualizados com operacoes atomicas porque varias threads entram e saem
/// ao mesmo tempo.
/// </summary>
public sealed class ConcurrencyTracker
{
    private int currentCount;
    private int maxCount;
    private long checksum;

    public int MaxObserved => Volatile.Read(ref maxCount);

    public long Checksum => Interlocked.Read(ref checksum);

    public void Enter()
    {
        int now = Interlocked.Increment(ref currentCount);

        // Atualiza o pico apenas enquanto o valor lido continuar menor que o atual.
        int observedMax = Volatile.Read(ref maxCount);
        while (now > observedMax)
        {
            int previous = Interlocked.CompareExchange(ref maxCount, now, observedMax);
            if (previous == observedMax)
            {
                return;
            }

            observedMax = previous;
        }
    }

    public void Exit()
    {
        Interlocked.Decrement(ref currentCount);
    }

    public void AddResult(long value)
    {
        Interlocked.Add(ref checksum, value);
    }
}
