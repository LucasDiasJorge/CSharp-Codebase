using System.Data;
using System.Data.SqlClient;
using TransactionPattern.Examples;

namespace TransactionPattern;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Transaction Pattern - ExecuteInTransactionAsync Demo          ║");
        Console.WriteLine("║     Demonstração de Execução Transacional Assíncrona              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Usa uma conexão em memória para demonstração
        // Em produção, use uma connection string real
        using IDbConnection connection = new MockDbConnection();

        try
        {
            // ═══════════════════════════════════════════════════════════════
            // Exemplo 1: Transferência Bancária
            // ═══════════════════════════════════════════════════════════════
            Console.WriteLine("═══ Exemplo 1: Transferência Bancária ═══\n");
            
            var transferService = new BankTransferService(connection);
            
            Console.WriteLine("Cenário 1: Transferência com sucesso");
            await transferService.TransferAsync(
                fromAccountId: 1,
                toAccountId: 2,
                amount: 500m
            );
            
            Console.WriteLine("\n" + new string('─', 70) + "\n");
            
            Console.WriteLine("Cenário 2: Transferência com saldo insuficiente (rollback automático)");
            try
            {
                await transferService.TransferAsync(
                    fromAccountId: 1,
                    toAccountId: 2,
                    amount: 99999m
                );
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ Erro esperado: {ex.Message}");
                Console.WriteLine("✓ Rollback automático executado - nenhuma mudança foi persistida!");
            }

            Console.WriteLine("\n" + new string('═', 70) + "\n");

            // ═══════════════════════════════════════════════════════════════
            // Exemplo 2: Processamento de Pedido
            // ═══════════════════════════════════════════════════════════════
            Console.WriteLine("═══ Exemplo 2: Processamento de Pedido ═══\n");
            
            var orderService = new OrderService(connection);
            
            Console.WriteLine("Cenário 1: Pedido processado com sucesso");
            await orderService.ProcessOrderAsync(
                customerId: 123,
                items: new List<OrderItem>
                {
                    new OrderItem(ProductId: 1, Quantity: 2, Price: 50m),
                    new OrderItem(ProductId: 2, Quantity: 1, Price: 100m)
                },
                paymentAmount: 200m
            );
            
            Console.WriteLine("\n" + new string('─', 70) + "\n");
            
            Console.WriteLine("Cenário 2: Pedido com pagamento insuficiente (rollback automático)");
            try
            {
                await orderService.ProcessOrderAsync(
                    customerId: 456,
                    items: new List<OrderItem>
                    {
                        new OrderItem(ProductId: 3, Quantity: 5, Price: 100m)
                    },
                    paymentAmount: 250m // Insuficiente: total = 500
                );
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ Erro esperado: {ex.Message}");
                Console.WriteLine("✓ Rollback automático executado - pedido não foi criado!");
            }

            Console.WriteLine("\n" + new string('═', 70) + "\n");

            // ═══════════════════════════════════════════════════════════════
            // Resumo dos Benefícios
            // ═══════════════════════════════════════════════════════════════
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    ✨ BENEFÍCIOS DEMONSTRADOS ✨                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("✓ Atomicidade: Tudo ou nada - garantia de consistência");
            Console.WriteLine("✓ Rollback Automático: Erros não deixam dados inconsistentes");
            Console.WriteLine("✓ Código Limpo: Lógica de negócio separada de infraestrutura");
            Console.WriteLine("✓ DRY: Sem duplicação de try/catch/commit/rollback");
            Console.WriteLine("✓ Segurança: using garante liberação de recursos");
            Console.WriteLine("✓ Async/Await: Não bloqueia threads durante operações longas");
            Console.WriteLine();
            Console.WriteLine("📖 Veja README.md para documentação completa dos benefícios!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n💥 Erro não esperado: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine("\n\nPressione qualquer tecla para sair...");
        Console.ReadKey();
    }
}

/// <summary>
/// Mock de conexão para demonstração sem necessidade de banco real.
/// </summary>
internal class MockDbConnection : IDbConnection
{
    public string ConnectionString { get; set; } = string.Empty;
    public int ConnectionTimeout => 30;
    public string Database => "MockDatabase";
    public ConnectionState State { get; private set; } = ConnectionState.Closed;

    public IDbTransaction BeginTransaction() => new MockDbTransaction();
    public IDbTransaction BeginTransaction(IsolationLevel il) => new MockDbTransaction();
    public void ChangeDatabase(string databaseName) { }
    public void Close() => State = ConnectionState.Closed;
    public IDbCommand CreateCommand() => new MockDbCommand();
    public void Open() => State = ConnectionState.Open;
    public void Dispose() { }
}

internal class MockDbTransaction : IDbTransaction
{
    public IDbConnection? Connection => null;
    public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
    
    public void Commit() { }
    public void Rollback() { }
    public void Dispose() { }
}

internal class MockDbCommand : IDbCommand
{
    public string CommandText { get; set; } = string.Empty;
    public int CommandTimeout { get; set; }
    public CommandType CommandType { get; set; }
    public IDbConnection? Connection { get; set; }
    public IDataParameterCollection Parameters => null!;
    public IDbTransaction? Transaction { get; set; }
    public UpdateRowSource UpdatedRowSource { get; set; }

    public void Cancel() { }
    public IDbDataParameter CreateParameter() => null!;
    public int ExecuteNonQuery() => 0;
    public IDataReader ExecuteReader() => null!;
    public IDataReader ExecuteReader(CommandBehavior behavior) => null!;
    public object? ExecuteScalar() => null;
    public void Prepare() { }
    public void Dispose() { }
}
