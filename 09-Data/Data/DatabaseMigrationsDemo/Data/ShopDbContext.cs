using DatabaseMigrationsDemo.Model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMigrationsDemo.Data;

public sealed class ShopDbContext : DbContext
{
    /// <summary>
    /// Caminho ancorado no diretório do binário, e não relativo ao diretório atual:
    /// com caminho relativo, <c>dotnet run --project ...</c> a partir da raiz do
    /// repositório criaria o arquivo lá, e não junto do projeto.
    /// </summary>
    public static readonly string ConnectionString =
        "Data Source=" + Path.Combine(AppContext.BaseDirectory, "migrations-demo.db");

    public ShopDbContext()
    {
    }

    public ShopDbContext(DbContextOptions<ShopDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(customer => customer.Id);
            entity.Property(customer => customer.Name).IsRequired().HasMaxLength(200);
            entity.Property(customer => customer.Email).IsRequired().HasMaxLength(200);

            // Seed declarativo: entra na migration como INSERT e e aplicado junto com
            // o schema. Mudar um registro aqui gera uma migration nova com o UPDATE
            // correspondente — o EF trata o seed como parte do schema versionado.
            entity.HasData(
                new Customer { Id = 1, Name = "Ana Souza", Email = "ana@example.com" },
                new Customer { Id = 2, Name = "Bruno Lima", Email = "bruno@example.com" });
        });
    }
}
