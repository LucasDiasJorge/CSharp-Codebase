using CQRSDemo.Commands;
using CQRSDemo.Infrastructure;
using CQRSDemo.Models;

namespace CQRSDemo.Handlers;

public class UpdateProductCommandHandler
{
    private readonly InMemoryDatabase _database;

    public UpdateProductCommandHandler(InMemoryDatabase database)
    {
        _database = database;
    }

    public bool Handle(UpdateProductCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ArgumentException("Nome do produto é obrigatório");

        if (command.Price < 0)
            throw new ArgumentException("Preço não pode ser negativo");

        if (command.Stock < 0)
            throw new ArgumentException("Estoque não pode ser negativo");

        Product product = new Product
        {
            Id = command.Id,
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            Stock = command.Stock
        };

        bool success = _database.UpdateProduct(product);

        if (success)
            Console.WriteLine($"[COMMAND] Produto atualizado: {product.Name} (ID: {product.Id})");
        else
            Console.WriteLine($"[COMMAND] Produto não encontrado para atualização (ID: {command.Id})");

        return success;
    }
}
