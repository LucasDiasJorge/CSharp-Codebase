namespace AsyncLockingDemo.Accounts;

/// <summary>
/// Cenário 4: exclusão mútua compatível com async. <c>SemaphoreSlim(1, 1)</c> é o
/// mutex assíncrono da BCL, e <c>WaitAsync</c> é o que separa este cenário dos
/// anteriores: quem espera não ocupa thread.
/// </summary>
public sealed class SemaphoreAccount : IAsyncAccount, IDisposable
{
    private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
    private readonly TimeSpan _ioDelay;
    private int _balance;

    public SemaphoreAccount(TimeSpan ioDelay)
    {
        _ioDelay = ioDelay;
    }

    public string Strategy => "SemaphoreSlim.WaitAsync()";

    public int Balance => _balance;

    public async Task DepositAsync(int amount, CancellationToken cancellationToken)
    {
        // WaitAsync devolve a thread enquanto a vaga não abre; a continuação é
        // reagendada quando o Release acontecer.
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Seção crítica inteira dentro do semáforo, await incluído: é isto que o
            // lock não consegue oferecer.
            int current = _balance;
            await Task.Delay(_ioDelay, cancellationToken).ConfigureAwait(false);
            _balance = current + amount;
        }
        finally
        {
            // finally obrigatório: sem ele, uma exceção deixa o semáforo fechado para
            // sempre. Demonstrado em ReleasePitfallDemo.
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _gate.Dispose();
    }
}
