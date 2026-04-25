using TransactionScript.Core;
using TransactionScript.Examples.TransferMoney.DTOs;

namespace TransactionScript.Examples.TransferMoney.Scripts;

/// <summary>
/// Script de transação para transferência de dinheiro
/// 
/// Este é o padrão Transaction Script clássico:
/// - Lógica procedural em um único método
/// - Todas as regras de negócio no script
/// - Acesso direto ao gateway de dados
/// </summary>
public class TransferMoneyScript : ITransactionScript<TransferMoneyInput, TransferMoneyOutput>
{
    private readonly IDataGateway _db;

    public TransferMoneyScript(IDataGateway db)
    {
        _db = db;
    }

    public ScriptResult<TransferMoneyOutput> Execute(TransferMoneyInput input)
    {
        Console.WriteLine("\n[TransferMoneyScript] Iniciando transferência...");

        // ========== VALIDAÇÃO DE ENTRADA ==========
        if (input.Amount <= 0)
        {
            return ScriptResult<TransferMoneyOutput>.Fail("Valor deve ser maior que zero");
        }

        if (input.FromAccountId == input.ToAccountId)
        {
            return ScriptResult<TransferMoneyOutput>.Fail("Conta origem e destino não podem ser iguais");
        }

        // ========== BUSCAR DADOS ==========
        var fromAccount = _db.GetAccount(input.FromAccountId);
        if (fromAccount == null)
        {
            return ScriptResult<TransferMoneyOutput>.Fail("Conta origem não encontrada");
        }

        var toAccount = _db.GetAccount(input.ToAccountId);
        if (toAccount == null)
        {
            return ScriptResult<TransferMoneyOutput>.Fail("Conta destino não encontrada");
        }

        // ========== VALIDAÇÃO DE NEGÓCIO ==========
        if (!fromAccount.IsActive)
        {
            return ScriptResult<TransferMoneyOutput>.Fail("Conta origem está inativa");
        }

        if (!toAccount.IsActive)
        {
            return ScriptResult<TransferMoneyOutput>.Fail("Conta destino está inativa");
        }

        if (fromAccount.Balance < input.Amount)
        {
            return ScriptResult<TransferMoneyOutput>.Fail(
                $"Saldo insuficiente. Disponível: {fromAccount.Balance:C}, Solicitado: {input.Amount:C}");
        }

        // ========== EXECUTAR TRANSAÇÃO ==========
        Console.WriteLine($"[TransferMoneyScript] Transferindo {input.Amount:C}");
        Console.WriteLine($"[TransferMoneyScript] De: {fromAccount.HolderName} ({fromAccount.Balance:C})");
        Console.WriteLine($"[TransferMoneyScript] Para: {toAccount.HolderName} ({toAccount.Balance:C})");

        // Debitar origem
        fromAccount.Balance -= input.Amount;
        _db.SaveAccount(fromAccount);

        // Creditar destino
        toAccount.Balance += input.Amount;
        _db.SaveAccount(toAccount);

        // Registrar transação
        var transaction = new TransactionData
        {
            Id = Guid.NewGuid(),
            FromAccountId = input.FromAccountId,
            ToAccountId = input.ToAccountId,
            Amount = input.Amount,
            Date = DateTime.UtcNow,
            Description = input.Description ?? "Transferência entre contas"
        };
        _db.SaveTransaction(transaction);

        // ========== RETORNAR RESULTADO ==========
        var output = new TransferMoneyOutput
        {
            TransactionId = transaction.Id,
            FromAccountNewBalance = fromAccount.Balance,
            ToAccountNewBalance = toAccount.Balance,
            TransactionDate = transaction.Date
        };

        Console.WriteLine($"[TransferMoneyScript] ✓ Transferência concluída - ID: {output.TransactionId}");

        return ScriptResult<TransferMoneyOutput>.Ok(output);
    }
}
