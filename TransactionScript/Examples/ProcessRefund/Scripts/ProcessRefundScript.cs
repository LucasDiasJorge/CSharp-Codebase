using TransactionScript.Core;
using TransactionScript.Examples.ProcessRefund.DTOs;

namespace TransactionScript.Examples.ProcessRefund.Scripts;

/// <summary>
/// Script de transação para processar reembolso
/// 
/// Demonstra script que afeta múltiplas entidades:
/// - Credita conta do cliente
/// - Restaura estoque do produto
/// - Registra transação
/// </summary>
public class ProcessRefundScript : ITransactionScript<ProcessRefundInput, ProcessRefundOutput>
{
    private readonly IDataGateway _db;

    public ProcessRefundScript(IDataGateway db)
    {
        _db = db;
    }

    public ScriptResult<ProcessRefundOutput> Execute(ProcessRefundInput input)
    {
        Console.WriteLine("\n[ProcessRefundScript] Processando reembolso...");

        // ========== VALIDAÇÃO DE ENTRADA ==========
        if (input.AccountId == Guid.Empty)
        {
            return ScriptResult<ProcessRefundOutput>.Fail("AccountId é obrigatório");
        }

        if (input.ProductId == Guid.Empty)
        {
            return ScriptResult<ProcessRefundOutput>.Fail("ProductId é obrigatório");
        }

        if (input.Quantity <= 0)
        {
            return ScriptResult<ProcessRefundOutput>.Fail("Quantidade deve ser maior que zero");
        }

        // ========== BUSCAR CONTA ==========
        var account = _db.GetAccount(input.AccountId);
        if (account == null)
        {
            return ScriptResult<ProcessRefundOutput>.Fail("Conta não encontrada");
        }

        if (!account.IsActive)
        {
            return ScriptResult<ProcessRefundOutput>.Fail("Conta está inativa");
        }

        // ========== BUSCAR PRODUTO ==========
        var product = _db.GetProduct(input.ProductId);
        if (product == null)
        {
            return ScriptResult<ProcessRefundOutput>.Fail("Produto não encontrado");
        }

        // ========== CALCULAR REEMBOLSO ==========
        var refundAmount = input.Quantity * product.Price;
        Console.WriteLine($"[ProcessRefundScript] Produto: {product.Name}");
        Console.WriteLine($"[ProcessRefundScript] Quantidade: {input.Quantity}");
        Console.WriteLine($"[ProcessRefundScript] Valor unitário: {product.Price:C}");
        Console.WriteLine($"[ProcessRefundScript] Valor total do reembolso: {refundAmount:C}");

        // ========== CREDITAR CONTA ==========
        Console.WriteLine($"[ProcessRefundScript] Saldo anterior: {account.Balance:C}");
        account.Balance += refundAmount;
        _db.SaveAccount(account);
        Console.WriteLine($"[ProcessRefundScript] Novo saldo: {account.Balance:C}");

        // ========== RESTAURAR ESTOQUE ==========
        Console.WriteLine($"[ProcessRefundScript] Estoque anterior: {product.Stock}");
        product.Stock += input.Quantity;
        _db.SaveProduct(product);
        Console.WriteLine($"[ProcessRefundScript] Novo estoque: {product.Stock}");

        // ========== REGISTRAR TRANSAÇÃO ==========
        var refundTransaction = new TransactionData
        {
            Id = Guid.NewGuid(),
            FromAccountId = Guid.Empty, // Sistema
            ToAccountId = input.AccountId,
            Amount = refundAmount,
            Date = DateTime.UtcNow,
            Description = $"Reembolso: {input.Reason}"
        };
        _db.SaveTransaction(refundTransaction);

        // ========== RETORNAR RESULTADO ==========
        var output = new ProcessRefundOutput
        {
            RefundId = refundTransaction.Id,
            RefundAmount = refundAmount,
            NewAccountBalance = account.Balance,
            NewProductStock = product.Stock,
            ProcessedAt = refundTransaction.Date
        };

        Console.WriteLine($"[ProcessRefundScript] ✓ Reembolso processado - ID: {output.RefundId}");

        return ScriptResult<ProcessRefundOutput>.Ok(output);
    }
}
