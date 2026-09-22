using EfCoreOptimisticConcurrencyDemo.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EfCoreOptimisticConcurrencyDemo.Data;

public sealed class ShopDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<UnguardedProduct> UnguardedProducts => Set<UnguardedProduct>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=concurrency-demo.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IsConcurrencyToken poe a coluna no WHERE de UPDATE e DELETE. E so isso que
        // transforma "gravar" em "gravar SE ninguem mexeu desde que eu li".
        modelBuilder.Entity<Product>()
            .Property(product => product.Version)
            .IsConcurrencyToken();
    }

    /// <summary>
    /// Incrementa o token antes de gravar. No SQL Server o banco faria isso sozinho com
    /// `rowversion`; no SQLite não há equivalente, então a responsabilidade é daqui —
    /// e precisa valer para TODA gravação, ou o token para de detectar conflito.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (EntityEntry<Product> entry in ChangeTracker.Entries<Product>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Version++;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
