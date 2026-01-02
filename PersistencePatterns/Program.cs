using PersistencePatterns.Examples.Repository.Entities;
using PersistencePatterns.Examples.Repository.Implementations;

namespace PersistencePatterns;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         PADRÕES DE PERSISTÊNCIA EM C#                     ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

        await DemonstrateRepositoryPattern();
        await DemonstrateUnitOfWorkPattern();
        await DemonstrateIdentityMapPattern();

        Console.WriteLine("\n✅ Demonstrações concluídas!");
    }

    static async Task DemonstrateRepositoryPattern()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("📦 REPOSITORY PATTERN");
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");

        var repository = new InMemoryProductRepository();

        // Adicionar produtos
        var product1 = Product.Create("Notebook", "Notebook Dell", 3500.00m, 10);
        var product2 = Product.Create("Mouse", "Mouse Logitech", 150.00m, 50);
        var product3 = Product.Create("Teclado", "Teclado Mecânico", 450.00m, 3);

        await repository.AddAsync(product1);
        await repository.AddAsync(product2);
        await repository.AddAsync(product3);

        Console.WriteLine("Produtos cadastrados:");
        var allProducts = await repository.GetAllAsync();
        foreach (var p in allProducts)
            Console.WriteLine($"  - {p.Name}: {p.Price:C} (Estoque: {p.StockQuantity})");

        // Buscar produtos com estoque baixo
        Console.WriteLine("\nProdutos com estoque baixo (<=5):");
        var lowStock = await repository.GetLowStockProductsAsync(5);
        foreach (var p in lowStock)
            Console.WriteLine($"  ⚠️ {p.Name}: {p.StockQuantity} unidades");

        Console.WriteLine();
    }

    static async Task DemonstrateUnitOfWorkPattern()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("🔄 UNIT OF WORK PATTERN");
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");

        using var unitOfWork = new Examples.UnitOfWork.Implementations.OrderUnitOfWork();

        Console.WriteLine("Cenário 1: Transação com COMMIT");
        await unitOfWork.BeginTransactionAsync();

        var order = Examples.UnitOfWork.Entities.Order.Create(Guid.NewGuid(), 1000.00m);
        await unitOfWork.Orders.AddAsync(order);

        var payment = Examples.UnitOfWork.Entities.Payment.Create(order.Id, 1000.00m);
        await unitOfWork.Payments.AddAsync(payment);

        payment.Approve();
        order.Confirm();

        await unitOfWork.SaveChangesAsync();
        await unitOfWork.CommitAsync();
        Console.WriteLine($"  Pedido {order.Id} criado com sucesso!\n");

        Console.WriteLine("Cenário 2: Transação com ROLLBACK");
        using var unitOfWork2 = new Examples.UnitOfWork.Implementations.OrderUnitOfWork();
        await unitOfWork2.BeginTransactionAsync();

        var order2 = Examples.UnitOfWork.Entities.Order.Create(Guid.NewGuid(), 500.00m);
        await unitOfWork2.Orders.AddAsync(order2);

        // Simular falha
        Console.WriteLine("  Simulando falha no pagamento...");
        await unitOfWork2.RollbackAsync();
        Console.WriteLine("  Pedido não foi persistido.\n");
    }

    static async Task DemonstrateIdentityMapPattern()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("🗺️ IDENTITY MAP PATTERN");
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");

        var identityMap = new Examples.IdentityMap.Implementations.IdentityMap<
            Examples.IdentityMap.Entities.Customer, Guid>();
        var repository = new Examples.IdentityMap.Implementations
            .CustomerRepositoryWithIdentityMap(identityMap);

        // Criar cliente
        var customerId = Guid.NewGuid();
        var customer = Examples.IdentityMap.Entities.Customer.Create(
            customerId, "João Silva", "joao@email.com");
        await repository.AddAsync(customer);

        Console.WriteLine("Acessando o mesmo cliente múltiplas vezes:");

        // Primeiro acesso - vai ao "banco"
        var customer1 = await repository.GetByIdAsync(customerId);

        // Segundo acesso - usa cache
        var customer2 = await repository.GetByIdAsync(customerId);

        // Terceiro acesso - usa cache
        var customer3 = await repository.GetByIdAsync(customerId);

        Console.WriteLine($"\n  Total de acessos ao banco: {repository.DatabaseAccessCount}");
        Console.WriteLine($"  Mesma instância? {ReferenceEquals(customer1, customer2) && ReferenceEquals(customer2, customer3)}");

        // Demonstrar consistência
        customer1!.UpdateName("João Silva Jr.");
        Console.WriteLine($"\n  Após modificar customer1:");
        Console.WriteLine($"    customer1.Name = {customer1.Name}");
        Console.WriteLine($"    customer2.Name = {customer2!.Name}");
        Console.WriteLine($"    customer3.Name = {customer3!.Name}");
        Console.WriteLine();
    }
}
