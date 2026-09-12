namespace AsyncLockingDemo.Accounts;

/// <summary>
/// Conta com uma seção crítica que atravessa um <c>await</c>: ler o saldo, esperar
/// a "ida ao banco de dados" e escrever o novo valor. Cada implementação protege
/// (ou deixa de proteger) essa sequência de um jeito diferente.
/// </summary>
public interface IAsyncAccount
{
    /// <summary>Nome da estratégia de sincronização, usado no relatório final.</summary>
    string Strategy { get; }

    /// <summary>Saldo acumulado após todos os depósitos.</summary>
    int Balance { get; }

    Task DepositAsync(int amount, CancellationToken cancellationToken);
}
