using CacheAside.Models;

namespace CacheAside.Interfaces
{
    public interface IProductService
    {
        Task<Product?> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
        Task<Product> CreateProductAsync(Product product);
        Task<Product?> UpdateProductAsync(int id, Product product);
        Task<bool> DeleteProductAsync(int id);
    }
}
