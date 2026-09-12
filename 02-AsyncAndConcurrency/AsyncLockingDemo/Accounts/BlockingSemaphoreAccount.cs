namespace AsyncLockingDemo.Accounts;

/// <summary>
/// Cenário 3: <see cref="SemaphoreSlim"/> com <c>Wait()</c> síncrono. Serve para
/// isolar a variável: trocar <c>lock</c> por semáforo não resolve nada enquanto a
/// espera continuar bloqueando a thread.
/// </summary>
public sealed class BlockingSemaphoreAccount : IAsyncAccount, IDisposable
{
    private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
    private readonly TimeSpan _ioDelay;
    private int _balance;

    public BlockingSemaphoreAccount(TimeSpan ioDelay)
    {
        _ioDelay = ioDelay;
    }

    public string Strategy => "SemaphoreSlim.Wait()";

    public int Balance => _balance;

    public Task DepositAsync(int amount, CancellationToken cancellationToken)
    {
        // Wait() bloqueia a thread chamadora até a vaga abrir, exatamente como o lock.
        _gate.Wait(cancellationToken);
        try
        {
            int current = _balance;
            Task.Delay(_ioDelay, cancellationToken).GetAwaiter().GetResult();
            _balance = current + amount;
        }
        finally
        {
            _gate.Release();
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _gate.Dispose();
    }
}
