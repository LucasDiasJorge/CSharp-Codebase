using System.Collections.Concurrent;

namespace Modules.Catalog.Internal;

/// <summary>
/// "Banco" do módulo de Catálogo. É <c>internal</c> de propósito: nenhum outro módulo
/// consegue sequer nomear este tipo, quanto mais ler ou escrever nele.
///
/// Em produção seria um schema próprio no mesmo banco físico — isolamento lógico, não
/// físico. A regra é a mesma: o módulo de Pedidos não faz SELECT na tabela de produtos.
/// </summary>
internal sealed class CatalogDatabase
{
    private readonly ConcurrentDictionary<string, ProductEntity> _products = new ConcurrentDictionary<string, ProductEntity>
    {
        ["ABC-1"] = new ProductEntity("ABC-1", "Teclado mecanico", 349.90m, 10, "fornecedor-interno-7"),
        ["XYZ-9"] = new ProductEntity("XYZ-9", "Monitor 27 polegadas", 1899.00m, 2, "fornecedor-interno-3")
    };

    public ProductEntity? Find(string sku) => _products.TryGetValue(sku, out ProductEntity? product) ? product : null;

    public IReadOnlyCollection<ProductEntity> All() => _products.Values.ToArray();

    public void Save(ProductEntity product) => _products[product.Sku] = product;
}

/// <summary>
/// Entidade interna. Tem campos que só interessam ao Catálogo — <see cref="SupplierCode"/>
/// não aparece no contrato público, e é justamente esse tipo de detalhe que não deve
/// vazar para os outros módulos.
/// </summary>
internal sealed class ProductEntity
{
    public ProductEntity(string sku, string name, decimal price, int stock, string supplierCode)
    {
        Sku = sku;
        Name = name;
        Price = price;
        Stock = stock;
        SupplierCode = supplierCode;
    }

    public string Sku { get; }

    public string Name { get; }

    public decimal Price { get; }

    public int Stock { get; private set; }

    public string SupplierCode { get; }

    public bool TryReserve(int quantity)
    {
        if (quantity > Stock)
        {
            return false;
        }

        Stock -= quantity;

        return true;
    }

    /// <summary>
    /// Devolve o estoque reservado. Necessário porque a reserva acontece antes de o
    /// pagamento ser decidido: se a cobrança falhar, a reserva precisa ser desfeita, ou
    /// o produto fica indisponível para sempre por causa de um pedido que não existe.
    /// </summary>
    public void Release(int quantity)
    {
        Stock += quantity;
    }
}
