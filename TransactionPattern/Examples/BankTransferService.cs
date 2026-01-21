using System.Data;
using TransactionPattern.Core;

namespace TransactionPattern.Examples;

/// <summary>
/// Serviço de transferência bancária que demonstra o uso do padrão de transação.
/// </summary>
public class BankTransferService : BaseRepository
{
    public BankTransferService(IDbConnection connection) : base(connection)
    {
    }

    /// <summary>
    /// Transfere dinheiro entre duas contas.
    /// Ambas as operações (débito e crédito) ocorrem na mesma transação.
    /// </summary>
    public async Task TransferAsync(int fromAccountId, int toAccountId, decimal amount)
    {
        await ExecuteInTransactionAsync(async transaction =>
        {
            // Debita da conta origem
            await DebitAccountAsync(fromAccountId, amount, transaction);
            
            // Credita na conta destino
            await CreditAccountAsync(toAccountId, amount, transaction);
            
            // Registra log da transferência
            await LogTransferAsync(fromAccountId, toAccountId, amount, transaction);
            
            Console.WriteLine($"✓ Transferência de {amount:C} concluída com sucesso!");
        });
    }

    private async Task DebitAccountAsync(int accountId, decimal amount, IDbTransaction transaction)
    {
        // Simula verificação de saldo
        var currentBalance = await GetBalanceAsync(accountId, transaction);
        
        if (currentBalance < amount)
        {
            throw new InvalidOperationException($"Saldo insuficiente na conta {accountId}. Saldo: {currentBalance:C}, Tentativa: {amount:C}");
        }

        // Simula débito
        using var command = _connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"UPDATE Accounts SET Balance = Balance - {amount} WHERE AccountId = {accountId}";
        // await command.ExecuteNonQueryAsync(); // Simulado
        
        await Task.Delay(100); // Simula operação assíncrona
        Console.WriteLine($"  - Debitado {amount:C} da conta {accountId}");
    }

    private async Task CreditAccountAsync(int accountId, decimal amount, IDbTransaction transaction)
    {
        // Simula crédito
        using var command = _connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"UPDATE Accounts SET Balance = Balance + {amount} WHERE AccountId = {accountId}";
        // await command.ExecuteNonQueryAsync(); // Simulado
        
        await Task.Delay(100); // Simula operação assíncrona
        Console.WriteLine($"  + Creditado {amount:C} na conta {accountId}");
    }

    private async Task LogTransferAsync(int fromAccountId, int toAccountId, decimal amount, IDbTransaction transaction)
    {
        // Simula log
        using var command = _connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"INSERT INTO TransferLogs (FromAccount, ToAccount, Amount, Date) VALUES ({fromAccountId}, {toAccountId}, {amount}, GETDATE())";
        // await command.ExecuteNonQueryAsync(); // Simulado
        
        await Task.Delay(50); // Simula operação assíncrona
        Console.WriteLine($"  ℹ Log de transferência registrado");
    }

    private async Task<decimal> GetBalanceAsync(int accountId, IDbTransaction transaction)
    {
        // Simula consulta de saldo
        await Task.Delay(50);
        
        // Retorna saldos fictícios para demonstração
        return accountId switch
        {
            1 => 1000m,
            2 => 500m,
            _ => 0m
        };
    }
}
