namespace SimpleWebAPI.Models;

public class ProductModel
{
    public long Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }

    [Price]
    public decimal Price { get; set; }

    public ProductModel(long id, string name, string description, decimal price)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
    }
}