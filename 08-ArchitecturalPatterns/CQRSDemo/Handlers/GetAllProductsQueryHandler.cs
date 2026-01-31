using CQRSDemo.Infrastructure;
using CQRSDemo.Queries;

namespace CQRSDemo.Handlers;

public class GetAllProductsQueryHandler
{
    private readonly InMemoryDatabase _database;

    public GetAllProductsQueryHandler(InMemoryDatabase database)
    {
        _database = database;
    }

    public List<ProductDto> Handle(GetAllProductsQuery query)
    {
        var products = _database.GetAllProducts();

        Console.WriteLine($"[QUERY] Retornando {products.Count} produtos");

        return products.Select(ProductDto.FromProduct).ToList();
    }
}
