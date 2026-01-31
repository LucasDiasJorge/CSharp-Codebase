using Microsoft.EntityFrameworkCore;
using CacheAside.Models;

namespace CacheAside.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed some initial data
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Category = "Electronics" },
                new Product { Id = 2, Name = "Mouse", Description = "Wireless gaming mouse", Price = 79.99m, Category = "Electronics" },
                new Product { Id = 3, Name = "Keyboard", Description = "Mechanical keyboard", Price = 129.99m, Category = "Electronics" },
                new Product { Id = 4, Name = "Monitor", Description = "4K gaming monitor", Price = 399.99m, Category = "Electronics" },
                new Product { Id = 5, Name = "Headphones", Description = "Noise-cancelling headphones", Price = 249.99m, Category = "Electronics" }
            );
        }
    }
}
