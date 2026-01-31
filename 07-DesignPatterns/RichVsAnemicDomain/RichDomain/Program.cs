using RichDomain.Models;
using RichDomain.Services;

namespace RichDomain;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== EXEMPLO DE DOMÍNIO RICO ===\n");
        
        var orderService = new OrderApplicationService();
        
        // Cria um pedido
        var order = orderService.CreateOrder("João Silva");
        Console.WriteLine($"✓ Pedido criado: {order.Id}");
        Console.WriteLine($"  Cliente: {order.CustomerName}");
        Console.WriteLine($"  Status: {order.Status}\n");
        
        // Adiciona itens
        Console.WriteLine("Adicionando itens...");
        order.AddItem("Notebook Dell", 1, 3500.00m);
        order.AddItem("Mouse Logitech", 2, 150.00m);
        order.AddItem("Teclado Mecânico", 1, 450.00m);
        
        foreach (var item in order.Items)
        {
            Console.WriteLine($"  - {item.ProductName}: {item.Quantity}x R$ {item.UnitPrice:F2} = R$ {item.Subtotal:F2}");
        }
        Console.WriteLine($"\n  Total: R$ {order.Total:F2}\n");
        
        // Aplica desconto
        Console.WriteLine("Aplicando desconto de 10%...");
        order.ApplyDiscount(10);
        Console.WriteLine($"  Total com desconto: R$ {order.Total:F2}\n");
        
        // Processa pedido
        Console.WriteLine("Processando pedido...");
        order.Process();
        Console.WriteLine($"  Status: {order.Status}\n");
        
        // Envia pedido
        Console.WriteLine("Enviando pedido...");
        order.Ship();
        Console.WriteLine($"  Status: {order.Status}\n");
        
        // Entrega pedido
        Console.WriteLine("Entregando pedido...");
        order.Deliver();
        Console.WriteLine($"  Status: {order.Status}\n");
        
        // ✅ DEMONSTRANDO PROTEÇÕES
        Console.WriteLine("\n=== DEMONSTRANDO PROTEÇÕES ===\n");
        
        // Cria novo pedido para testes
        var testOrder = Order.Create("Maria Santos");
        testOrder.AddItem("Produto Teste", 1, 100.00m);
        
        Console.WriteLine("✅ PROTEÇÃO 1: Tentando criar pedido com nome vazio");
        try
        {
            var invalidOrder = Order.Create(""); // ❌ Vai lançar exceção
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"   ✓ Bloqueado: {ex.Message}\n");
        }
        
        Console.WriteLine("✅ PROTEÇÃO 2: Tentando adicionar item com quantidade negativa");
        try
        {
            testOrder.AddItem("Produto", -5, 100); // ❌ Vai lançar exceção
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"   ✓ Bloqueado: {ex.Message}\n");
        }
        
        Console.WriteLine("✅ PROTEÇÃO 3: Tentando aplicar desconto inválido");
        try
        {
            testOrder.ApplyDiscount(150); // ❌ Vai lançar exceção
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"   ✓ Bloqueado: {ex.Message}\n");
        }
        
        Console.WriteLine("✅ PROTEÇÃO 4: Tentando cancelar pedido processado");
        testOrder.Process(); // Processa o pedido
        try
        {
            testOrder.Cancel(); // ❌ Não pode cancelar depois de processar
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"   ✓ Bloqueado: {ex.Message}\n");
        }
        
        Console.WriteLine("✅ PROTEÇÃO 5: Total sempre consistente");
        var consistentOrder = Order.Create("Teste Consistência");
        consistentOrder.AddItem("Item 1", 2, 50.00m); // R$ 100
        Console.WriteLine($"   Total antes do desconto: R$ {consistentOrder.Total:F2}");
        consistentOrder.ApplyDiscount(20); // 20% de desconto
        Console.WriteLine($"   Total depois do desconto: R$ {consistentOrder.Total:F2}");
        Console.WriteLine($"   ✓ Total recalculado automaticamente!\n");
        
        Console.WriteLine("✅ PROTEÇÃO 6: Coleções protegidas");
        try
        {
            // Items é ReadOnly - não pode adicionar diretamente
            // testOrder.Items.Add(...); // ❌ Não compila!
            Console.WriteLine("   ✓ Coleção Items é read-only - só pode modificar via métodos\n");
        }
        catch { }
        
        Console.WriteLine("✅ PROTEÇÃO 7: Propriedades imutáveis");
        try
        {
            // Propriedades com setter privado
            // testOrder.Status = OrderStatus.Delivered; // ❌ Não compila!
            // testOrder.Total = 9999; // ❌ Não compila!
            Console.WriteLine("   ✓ Propriedades protegidas - só podem ser mudadas internamente\n");
        }
        catch { }
        
        Console.WriteLine("\n=== VANTAGENS DO DOMÍNIO RICO ===");
        Console.WriteLine("✓ Impossível criar estados inválidos");
        Console.WriteLine("✓ Lógica de negócio encapsulada no domínio");
        Console.WriteLine("✓ Total sempre consistente (calculado automaticamente)");
        Console.WriteLine("✓ Validações sempre aplicadas");
        Console.WriteLine("✓ Código expressivo e fácil de entender");
        Console.WriteLine("✓ Fácil de testar");
        Console.WriteLine("✓ Manutenção simplificada");
        
        Console.WriteLine("\n=== COMPARAÇÃO ===");
        Console.WriteLine("Domínio Anêmico:");
        Console.WriteLine("  order.Total = -1000; // ✗ Permitido!");
        Console.WriteLine("\nDomínio Rico:");
        Console.WriteLine("  // order.Total = -1000; // ✓ Não compila!");
        Console.WriteLine("  // Total é calculado automaticamente e não pode ser modificado\n");
    }
}
