using CQRSDemo.Commands;
using CQRSDemo.Handlers;
using CQRSDemo.Infrastructure;
using CQRSDemo.Queries;

namespace CQRSDemo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("====================================");
        Console.WriteLine("   CQRS Demo - Sistema de Produtos");
        Console.WriteLine("====================================\n");

        // Setup - Criar database e handlers
        InMemoryDatabase database = new InMemoryDatabase();

        // Command Handlers (Write Operations)
        CreateProductCommandHandler createProductHandler = new CreateProductCommandHandler(database);
        UpdateProductCommandHandler updateProductHandler = new UpdateProductCommandHandler(database);
        DeleteProductCommandHandler deleteProductHandler = new DeleteProductCommandHandler(database);

        // Query Handlers (Read Operations)
        GetProductByIdQueryHandler getProductByIdHandler = new GetProductByIdQueryHandler(database);
        GetAllProductsQueryHandler getAllProductsHandler = new GetAllProductsQueryHandler(database);
        GetLowStockProductsQueryHandler getLowStockProductsHandler = new GetLowStockProductsQueryHandler(database);

        Console.WriteLine("\n--- PARTE 1: Criando Produtos (COMMANDS) ---\n");

        // Criar produtos usando Commands
        Guid product1Id = createProductHandler.Handle(new CreateProductCommand
        {
            Name = "Notebook Dell",
            Description = "Notebook Dell Inspiron 15, 16GB RAM, 512GB SSD",
            Price = 3500.00m,
            Stock = 15
        });

        Guid product2Id = createProductHandler.Handle(new CreateProductCommand
        {
            Name = "Mouse Logitech",
            Description = "Mouse sem fio Logitech MX Master 3",
            Price = 350.00m,
            Stock = 8
        });

        Guid product3Id = createProductHandler.Handle(new CreateProductCommand
        {
            Name = "Teclado Mecânico",
            Description = "Teclado mecânico RGB, switches blue",
            Price = 450.00m,
            Stock = 25
        });

        Console.WriteLine("\n--- PARTE 2: Consultando Produtos (QUERIES) ---\n");

        // Buscar todos os produtos
        List<CQRSDemo.Queries.ProductDto> allProducts = getAllProductsHandler.Handle(new GetAllProductsQuery());
        
        Console.WriteLine("\nTodos os produtos:");
        foreach (CQRSDemo.Queries.ProductDto product in allProducts)
        {
            Console.WriteLine($"  - {product.Name}: R$ {product.Price:N2} (Estoque: {product.Stock})");
        }

        // Buscar produto específico
        Console.WriteLine("\n");
        CQRSDemo.Queries.ProductDto? specificProduct = getProductByIdHandler.Handle(new GetProductByIdQuery 
        { 
            Id = product1Id 
        });
        
        if (specificProduct != null)
        {
            Console.WriteLine($"\nDetalhes do produto:");
            Console.WriteLine($"  Nome: {specificProduct.Name}");
            Console.WriteLine($"  Descrição: {specificProduct.Description}");
            Console.WriteLine($"  Preço: R$ {specificProduct.Price:N2}");
            Console.WriteLine($"  Estoque: {specificProduct.Stock}");
        }

        Console.WriteLine("\n--- PARTE 3: Atualizando Produto (COMMAND) ---\n");

        // Atualizar produto
        updateProductHandler.Handle(new UpdateProductCommand
        {
            Id = product2Id,
            Name = "Mouse Logitech MX Master 3",
            Description = "Mouse sem fio Logitech MX Master 3 - Edição Especial",
            Price = 380.00m,
            Stock = 12
        });

        Console.WriteLine("\n--- PARTE 4: Query Especializada ---\n");

        // Buscar produtos com estoque baixo
        List<CQRSDemo.Queries.ProductDto> lowStockProducts = getLowStockProductsHandler.Handle(new GetLowStockProductsQuery 
        { 
            Threshold = 10 
        });
        
        Console.WriteLine("\nProdutos com estoque baixo:");
        foreach (CQRSDemo.Queries.ProductDto product in lowStockProducts)
        {
            Console.WriteLine($"  - {product.Name}: {product.Stock} unidades");
        }

        Console.WriteLine("\n--- PARTE 5: Deletando Produto (COMMAND) ---\n");

        // Deletar produto
        deleteProductHandler.Handle(new DeleteProductCommand 
        { 
            Id = product3Id 
        });

        // Verificar produtos restantes
        Console.WriteLine("\n");
        List<CQRSDemo.Queries.ProductDto> remainingProducts = getAllProductsHandler.Handle(new GetAllProductsQuery());
        Console.WriteLine($"\nProdutos restantes: {remainingProducts.Count}");
        foreach (CQRSDemo.Queries.ProductDto product in remainingProducts)
        {
            Console.WriteLine($"  - {product.Name}");
        }

        Console.WriteLine("\n====================================");
        Console.WriteLine("   CQRS Demo Concluído!");
        Console.WriteLine("====================================\n");

        Console.WriteLine("Principais conceitos demonstrados:");
        Console.WriteLine("✓ Separação entre Commands (write) e Queries (read)");
        Console.WriteLine("✓ Handlers especializados para cada operação");
        Console.WriteLine("✓ DTOs para transferência de dados");
        Console.WriteLine("✓ Validações em Command Handlers");
        Console.WriteLine("✓ Queries otimizadas para leitura");
    }
}
