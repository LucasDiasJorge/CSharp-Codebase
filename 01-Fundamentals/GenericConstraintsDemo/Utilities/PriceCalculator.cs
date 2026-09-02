using System.Collections.Generic;
using GenericConstraintsDemo.Contracts;

namespace GenericConstraintsDemo.Utilities;

public static class PriceCalculator
{
    public static decimal SumPrices<TItem>(IEnumerable<TItem> items)
        where TItem : IPricedItem
    {
        decimal total = 0m;

        foreach (TItem item in items)
        {
            total += item.Price;
        }

        return total;
    }
}
