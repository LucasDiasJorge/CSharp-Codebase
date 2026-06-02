namespace PredicateAggregationDemo.Models;

public sealed class ProductFilter
{
    public ProductFilter(bool activeOnly, string? searchTerm, decimal? maximumPrice)
    {
        ActiveOnly = activeOnly;
        SearchTerm = searchTerm;
        MaximumPrice = maximumPrice;
    }

    public bool ActiveOnly { get; }

    public string? SearchTerm { get; }

    public decimal? MaximumPrice { get; }
}
