using System.Data;
using TransactionPattern.Core;

namespace TransactionPattern.Examples;

/// <summary>
/// Serviço de pedidos que demonstra múltiplas operações em uma transação.
/// </summary>
public class OrderService : BaseRepository
{
    public OrderService(IDbConnection connection) : base(connection)
    {
    }

    /// <summary>
    /// Processa um pedido completo: cria pedido, adiciona itens, atualiza estoque e registra pagamento.
    /// Todas as operações acontecem atomicamente.
    /// </summary>
    public async Task ProcessOrderAsync(int customerId, List<OrderItem> items, decimal paymentAmount)
    {
        await ExecuteInTransactionAsync(async transaction =>
        {
            // 1. Cria o pedido
            int orderId = await CreateOrderAsync(customerId, transaction);
            
            // 2. Adiciona os itens do pedido
            decimal totalAmount = 0;
            foreach (var item in items)
            {
                await AddOrderItemAsync(orderId, item, transaction);
                totalAmount += item.Price * item.Quantity;
                
                // 3. Atualiza o estoque
                await UpdateInventoryAsync(item.ProductId, -item.Quantity, transaction);
            }
            
            // 4. Valida o pagamento
            if (paymentAmount < totalAmount)
            {
                throw new InvalidOperationException($"Pagamento insuficiente. Total: {totalAmount:C}, Pago: {paymentAmount:C}");
            }
            
            // 5. Registra o pagamento
            await RecordPaymentAsync(orderId, paymentAmount, transaction);
            
            Console.WriteLine($"✓ Pedido #{orderId} processado com sucesso! Total: {totalAmount:C}");
        });
    }

    private async Task<int> CreateOrderAsync(int customerId, IDbTransaction transaction)
    {
        await Task.Delay(50); // Simula operação assíncrona
        int orderId = new Random().Next(1000, 9999);
        Console.WriteLine($"  ✓ Pedido #{orderId} criado para cliente {customerId}");
        return orderId;
    }

    private async Task AddOrderItemAsync(int orderId, OrderItem item, IDbTransaction transaction)
    {
        await Task.Delay(30); // Simula operação assíncrona
        Console.WriteLine($"    • Item adicionado: {item.ProductId} x{item.Quantity} = {item.Price * item.Quantity:C}");
    }

    private async Task UpdateInventoryAsync(int productId, int quantityChange, IDbTransaction transaction)
    {
        await Task.Delay(40); // Simula operação assíncrona
        
        // Simula verificação de estoque
        int currentStock = 100; // Simulado
        if (currentStock + quantityChange < 0)
        {
            throw new InvalidOperationException($"Estoque insuficiente para produto {productId}");
        }
        
        Console.WriteLine($"    ↓ Estoque atualizado para produto {productId}: {quantityChange}");
    }

    private async Task RecordPaymentAsync(int orderId, decimal amount, IDbTransaction transaction)
    {
        await Task.Delay(50); // Simula operação assíncrona
        Console.WriteLine($"  💳 Pagamento de {amount:C} registrado para pedido #{orderId}");
    }
}

public record OrderItem(int ProductId, int Quantity, decimal Price);
