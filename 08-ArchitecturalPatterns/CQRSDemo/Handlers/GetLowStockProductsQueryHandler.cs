using CQRSDemo.Infrastructure;
using CQRSDemo.Queries;

namespace CQRSDemo.Handlers;

public class GetLowStockProductsQueryHandler
{
    private readonly InMemoryDatabase _database;

    public GetLowStockProductsQueryHandler(InMemoryDatabase database)
    {
        _database = database;
    }

    public List<ProductDto> Handle(GetLowStockProductsQuery query)
    {
        var products = _database.GetAllProducts()
            .Where(p => p.Stock <= query.Threshold)
            .ToList();

        Console.WriteLine($"[QUERY] Encontrados {products.Count} produtos com estoque baixo (< {query.Threshold})");

        return products.Select(ProductDto.FromProduct).ToList();
    }
}
