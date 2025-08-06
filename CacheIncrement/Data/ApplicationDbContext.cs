using Microsoft.EntityFrameworkCore;
using CacheIncrement.Models;

namespace CacheIncrement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Counter> Counters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Counter>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(255);
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.LastUpdated).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                
                entity.HasIndex(e => e.LastUpdated);
            });
        }
    }
}
