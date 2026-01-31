using CQRSDemo.Infrastructure;
using CQRSDemo.Queries;

namespace CQRSDemo.Handlers;

public class GetProductByIdQueryHandler
{
    private readonly InMemoryDatabase _database;

    public GetProductByIdQueryHandler(InMemoryDatabase database)
    {
        _database = database;
    }

    public ProductDto? Handle(GetProductByIdQuery query)
    {
        var product = _database.GetProductById(query.Id);

        if (product == null)
        {
            Console.WriteLine($"[QUERY] Produto não encontrado (ID: {query.Id})");
            return null;
        }

        Console.WriteLine($"[QUERY] Produto encontrado: {product.Name}");
        return ProductDto.FromProduct(product);
    }
}
