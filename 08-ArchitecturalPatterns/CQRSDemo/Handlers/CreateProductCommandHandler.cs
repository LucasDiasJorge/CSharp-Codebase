using CQRSDemo.Commands;
using CQRSDemo.Infrastructure;
using CQRSDemo.Models;

namespace CQRSDemo.Handlers;

public class CreateProductCommandHandler
{
    private readonly InMemoryDatabase _database;

    public CreateProductCommandHandler(InMemoryDatabase database)
    {
        _database = database;
    }

    public Guid Handle(CreateProductCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ArgumentException("Nome do produto é obrigatório");

        if (command.Price < 0)
            throw new ArgumentException("Preço não pode ser negativo");

        if (command.Stock < 0)
            throw new ArgumentException("Estoque não pode ser negativo");

        Product product = new Product
        {
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            Stock = command.Stock
        };

        _database.AddProduct(product);

        Console.WriteLine($"[COMMAND] Produto criado: {product.Name} (ID: {product.Id})");

        return product.Id;
    }
}
