using AnemicDomain.Models;
using AnemicDomain.Services;

namespace AnemicDomain;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== EXEMPLO DE DOMÍNIO ANÊMICO ===\n");
        
        var orderService = new OrderService();
        
        // Cria um pedido
        var order = orderService.CreateOrder("João Silva");
        Console.WriteLine($"✓ Pedido criado: {order.Id}");
        Console.WriteLine($"  Cliente: {order.CustomerName}");
        Console.WriteLine($"  Status: {order.Status}\n");
        
        // Adiciona itens
        Console.WriteLine("Adicionando itens...");
        orderService.AddItem(order, "Notebook Dell", 1, 3500.00m);
        orderService.AddItem(order, "Mouse Logitech", 2, 150.00m);
        orderService.AddItem(order, "Teclado Mecânico", 1, 450.00m);
        
        foreach (var item in order.Items)
        {
            Console.WriteLine($"  - {item.ProductName}: {item.Quantity}x R$ {item.UnitPrice:F2} = R$ {item.Subtotal:F2}");
        }
        Console.WriteLine($"\n  Total: R$ {order.Total:F2}\n");
        
        // Aplica desconto
        Console.WriteLine("Aplicando desconto de 10%...");
        orderService.ApplyDiscount(order, 10);
        Console.WriteLine($"  Total com desconto: R$ {order.Total:F2}\n");
        
        // Processa pedido
        Console.WriteLine("Processando pedido...");
        orderService.ProcessOrder(order);
        Console.WriteLine($"  Status: {order.Status}\n");
        
        // ❌ PROBLEMA: Podemos modificar diretamente sem validação!
        Console.WriteLine("=== DEMONSTRANDO PROBLEMAS ===\n");
        
        Console.WriteLine("❌ PROBLEMA 1: Modificação sem validação");
        order.CustomerName = ""; // Inválido, mas permitido!
        Console.WriteLine($"   Nome do cliente modificado para: '{order.CustomerName}' (INVÁLIDO!)\n");
        
        Console.WriteLine("❌ PROBLEMA 2: Total pode ficar dessincronizado");
        order.Items[0].Quantity = 10; // Mudou a quantidade
        Console.WriteLine($"   Quantidade mudou, mas total continua: R$ {order.Total:F2}");
        Console.WriteLine($"   Deveria ser: R$ {(10 * 3500 + 2 * 150 + 450) * 0.9:F2} (ERRADO!)\n");
        
        Console.WriteLine("❌ PROBLEMA 3: Estado inválido é possível");
        order.Status = OrderStatus.Delivered; // Pulou etapas!
        Console.WriteLine($"   Status mudou direto para: {order.Status} (sem validação!)\n");
        
        Console.WriteLine("❌ PROBLEMA 4: Dados inválidos aceitos");
        var invalidItem = new OrderItem
        {
            ProductName = "",
            Quantity = -5, // Negativo!
            UnitPrice = -100, // Negativo!
            Subtotal = 999999 // Inconsistente!
        };
        order.Items.Add(invalidItem);
        Console.WriteLine("   Item inválido adicionado sem validação!\n");
        
        Console.WriteLine("\n=== CONCLUSÃO ===");
        Console.WriteLine("No domínio anêmico:");
        Console.WriteLine("✗ Não há proteção contra estados inválidos");
        Console.WriteLine("✗ Lógica de negócio está espalhada em serviços");
        Console.WriteLine("✗ Fácil esquecer de validar ou recalcular");
        Console.WriteLine("✗ Difícil de manter e testar");
        Console.WriteLine("\nVeja o exemplo de Domínio Rico para a solução correta!");
    }
}
