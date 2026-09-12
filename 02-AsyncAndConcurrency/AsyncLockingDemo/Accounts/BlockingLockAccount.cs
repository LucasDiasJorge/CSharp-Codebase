namespace AsyncLockingDemo.Accounts;

/// <summary>
/// Cenário 2: a correção "óbvia" com <c>lock</c>. Como não se pode usar <c>await</c>
/// dentro de um <c>lock</c>, a operação assíncrona vira uma chamada bloqueante — o
/// saldo fica correto e a thread pool paga a conta.
/// </summary>
public sealed class BlockingLockAccount : IAsyncAccount
{
    private readonly object _gate = new object();
    private readonly TimeSpan _ioDelay;
    private int _balance;

    public BlockingLockAccount(TimeSpan ioDelay)
    {
        _ioDelay = ioDelay;
    }

    public string Strategy => "lock + sync-over-async";

    public int Balance => _balance;

    public Task DepositAsync(int amount, CancellationToken cancellationToken)
    {
        // O método não é async de propósito: `await` dentro de `lock` é erro de
        // compilação (CS1996), porque a continuação pode voltar em outra thread e
        // Monitor exige que quem libera seja quem adquiriu.
        lock (_gate)
        {
            int current = _balance;

            // Sync-over-async: a thread fica parada segurando o Monitor. É a linha
            // que este cenário existe para medir.
            Task.Delay(_ioDelay, cancellationToken).GetAwaiter().GetResult();

            _balance = current + amount;
        }

        return Task.CompletedTask;
    }
}
