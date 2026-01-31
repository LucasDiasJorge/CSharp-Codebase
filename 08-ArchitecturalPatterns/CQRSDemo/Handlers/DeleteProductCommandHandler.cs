using CQRSDemo.Commands;
using CQRSDemo.Infrastructure;

namespace CQRSDemo.Handlers;

public class DeleteProductCommandHandler
{
    private readonly InMemoryDatabase _database;

    public DeleteProductCommandHandler(InMemoryDatabase database)
    {
        _database = database;
    }

    public bool Handle(DeleteProductCommand command)
    {
        bool success = _database.DeleteProduct(command.Id);

        if (success)
            Console.WriteLine($"[COMMAND] Produto deletado (ID: {command.Id})");
        else
            Console.WriteLine($"[COMMAND] Produto não encontrado para deleção (ID: {command.Id})");

        return success;
    }
}
