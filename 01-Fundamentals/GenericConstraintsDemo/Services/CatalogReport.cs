using System;
using System.Collections.Generic;
using GenericConstraintsDemo.Contracts;
using GenericConstraintsDemo.Utilities;

namespace GenericConstraintsDemo.Services;

public sealed class CatalogReport<TEntity>
    where TEntity : class, IEntity, IPricedItem
{
    private readonly IReadableCatalog<TEntity> catalog;

    public CatalogReport(IReadableCatalog<TEntity> catalog)
    {
        this.catalog = catalog;
    }

    public void PrintSummary()
    {
        IReadOnlyCollection<TEntity> entities = catalog.GetAll();
        decimal total = PriceCalculator.SumPrices(entities);

        Console.WriteLine($"Itens no catalogo: {entities.Count}");
        Console.WriteLine($"Valor total: {total:C}");
    }
}
