using Microsoft.EntityFrameworkCore;
using CacheAside.Data;
using CacheAside.Interfaces;
using CacheAside.Models;

namespace CacheAside.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching product from database with ID: {Id}", id);
            
            // Simulate database delay
            await Task.Delay(100);
            
            return await _context.Products.FindAsync(id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all products from database");
            
            // Simulate database delay
            await Task.Delay(200);
            
            return await _context.Products.ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            _logger.LogInformation("Fetching products from database by category: {Category}", category);
            
            // Simulate database delay
            await Task.Delay(150);
            
            return await _context.Products
                .Where(p => p.Category.ToLower() == category.ToLower())
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _logger.LogInformation("Creating new product in database: {ProductName}", product.Name);
            
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            
            return product;
        }

        public async Task<Product?> UpdateAsync(int id, Product product)
        {
            _logger.LogInformation("Updating product in database with ID: {Id}", id);
            
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Category = product.Category;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingProduct;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting product from database with ID: {Id}", id);
            
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
