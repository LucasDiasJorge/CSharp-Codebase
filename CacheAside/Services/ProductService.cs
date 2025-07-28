using CacheAside.Interfaces;
using CacheAside.Models;

namespace CacheAside.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ProductService> _logger;
        
        // Cache key constants
        private const string PRODUCT_CACHE_KEY = "product:{0}";
        private const string ALL_PRODUCTS_CACHE_KEY = "products:all";
        private const string CATEGORY_CACHE_KEY = "products:category:{0}";

        public ProductService(
            IProductRepository productRepository, 
            ICacheService cacheService,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            _logger.LogInformation("Getting product with ID: {Id}", id);
            
            var cacheKey = string.Format(PRODUCT_CACHE_KEY, id);
            
            // 1. Try to get from cache first (Cache Aside pattern)
            var cachedProduct = await _cacheService.GetAsync<Product>(cacheKey);
            if (cachedProduct != null)
            {
                _logger.LogInformation("Product found in cache for ID: {Id}", id);
                return cachedProduct;
            }

            // 2. Cache miss - get from database
            _logger.LogInformation("Product not found in cache, fetching from database for ID: {Id}", id);
            var product = await _productRepository.GetByIdAsync(id);
            
            // 3. Store in cache for future requests
            if (product != null)
            {
                await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(10));
                _logger.LogInformation("Product cached for ID: {Id}", id);
            }

            return product;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            _logger.LogInformation("Getting all products");
            
            // 1. Try to get from cache first
            var cachedProducts = await _cacheService.GetAsync<IEnumerable<Product>>(ALL_PRODUCTS_CACHE_KEY);
            if (cachedProducts != null)
            {
                _logger.LogInformation("All products found in cache");
                return cachedProducts;
            }

            // 2. Cache miss - get from database
            _logger.LogInformation("All products not found in cache, fetching from database");
            var products = await _productRepository.GetAllAsync();
            
            // 3. Store in cache for future requests
            await _cacheService.SetAsync(ALL_PRODUCTS_CACHE_KEY, products, TimeSpan.FromMinutes(5));
            _logger.LogInformation("All products cached");

            return products;
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            _logger.LogInformation("Getting products by category: {Category}", category);
            
            var cacheKey = string.Format(CATEGORY_CACHE_KEY, category.ToLower());
            
            // 1. Try to get from cache first
            var cachedProducts = await _cacheService.GetAsync<IEnumerable<Product>>(cacheKey);
            if (cachedProducts != null)
            {
                _logger.LogInformation("Products found in cache for category: {Category}", category);
                return cachedProducts;
            }

            // 2. Cache miss - get from database
            _logger.LogInformation("Products not found in cache, fetching from database for category: {Category}", category);
            var products = await _productRepository.GetByCategoryAsync(category);
            
            // 3. Store in cache for future requests
            await _cacheService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(7));
            _logger.LogInformation("Products cached for category: {Category}", category);

            return products;
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _logger.LogInformation("Creating new product: {ProductName}", product.Name);
            
            // 1. Create in database
            var createdProduct = await _productRepository.CreateAsync(product);
            
            // 2. Invalidate related cache entries (Cache Aside pattern)
            await _cacheService.RemoveAsync(ALL_PRODUCTS_CACHE_KEY);
            await _cacheService.RemoveAsync(string.Format(CATEGORY_CACHE_KEY, product.Category.ToLower()));
            
            _logger.LogInformation("Created product and invalidated related cache entries");
            
            return createdProduct;
        }

        public async Task<Product?> UpdateProductAsync(int id, Product product)
        {
            _logger.LogInformation("Updating product with ID: {Id}", id);
            
            // 1. Update in database
            var updatedProduct = await _productRepository.UpdateAsync(id, product);
            
            if (updatedProduct != null)
            {
                // 2. Update cache with new data
                var cacheKey = string.Format(PRODUCT_CACHE_KEY, id);
                await _cacheService.SetAsync(cacheKey, updatedProduct, TimeSpan.FromMinutes(10));
                
                // 3. Invalidate related cache entries
                await _cacheService.RemoveAsync(ALL_PRODUCTS_CACHE_KEY);
                await _cacheService.RemoveAsync(string.Format(CATEGORY_CACHE_KEY, product.Category.ToLower()));
                
                _logger.LogInformation("Updated product and refreshed cache entries");
            }

            return updatedProduct;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            _logger.LogInformation("Deleting product with ID: {Id}", id);
            
            // 1. Get product to know its category before deletion
            var productToDelete = await _productRepository.GetByIdAsync(id);
            
            // 2. Delete from database
            var deleted = await _productRepository.DeleteAsync(id);
            
            if (deleted && productToDelete != null)
            {
                // 3. Remove from cache
                var cacheKey = string.Format(PRODUCT_CACHE_KEY, id);
                await _cacheService.RemoveAsync(cacheKey);
                
                // 4. Invalidate related cache entries
                await _cacheService.RemoveAsync(ALL_PRODUCTS_CACHE_KEY);
                await _cacheService.RemoveAsync(string.Format(CATEGORY_CACHE_KEY, productToDelete.Category.ToLower()));
                
                _logger.LogInformation("Deleted product and removed from cache");
            }

            return deleted;
        }
    }
}
