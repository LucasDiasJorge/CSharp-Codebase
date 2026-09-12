namespace AsyncLockingDemo.Accounts;

/// <summary>
/// Cenário 1: nenhuma sincronização. Existe para reproduzir a race condition,
/// não para ser copiado.
/// </summary>
public sealed class UnsafeAccount : IAsyncAccount
{
    private readonly TimeSpan _ioDelay;
    private int _balance;

    public UnsafeAccount(TimeSpan ioDelay)
    {
        _ioDelay = ioDelay;
    }

    public string Strategy => "Sem sincronizacao";

    public int Balance => _balance;

    public async Task DepositAsync(int amount, CancellationToken cancellationToken)
    {
        // Read-modify-write partido em dois: entre a leitura e a escrita existe um
        // await, e o await devolve a thread. Toda task que entrar nessa janela lê o
        // mesmo saldo e sobrescreve o trabalho das outras.
        int current = _balance;

        await Task.Delay(_ioDelay, cancellationToken).ConfigureAwait(false);

        _balance = current + amount;
    }
}
