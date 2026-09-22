using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MultiTenantDataIsolationDemo.Model;

namespace MultiTenantDataIsolationDemo.Data;

public sealed class BillingDbContext : DbContext
{
    private readonly TenantContext _tenant;

    public BillingDbContext(TenantContext tenant)
    {
        _tenant = tenant;
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=multitenant-demo.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(invoice => invoice.Id);

            // Filtro global: entra em TODA consulta LINQ sobre Invoice, sem que o
            // chamador precise lembrar. E a diferenca entre "isolamento por disciplina"
            // e "isolamento por padrao".
            //
            // Repare que o filtro le _tenant.CurrentTenantId em tempo de consulta: por
            // isso trocar o tenant no contexto muda o que as consultas enxergam.
            entity.HasQueryFilter(invoice => invoice.TenantId == _tenant.CurrentTenantId);

            // Indice composto com TenantId PRIMEIRO. A ordem nao e detalhe: toda
            // consulta filtra por tenant, entao a coluna mais seletiva para o plano e
            // ela. Um indice (Customer, TenantId) seria quase inutil aqui.
            entity.HasIndex(invoice => new { invoice.TenantId, invoice.Customer })
                .HasDatabaseName("IX_Invoices_TenantId_Customer");
        });
    }

    /// <summary>
    /// Preenche o tenant nas entidades novas. Sem isto, esquecer de atribuir
    /// <c>TenantId</c> grava uma linha que nenhum tenant enxerga — ou, pior, que cai no
    /// tenant errado se o valor default coincidir.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (EntityEntry<ITenantOwned> entry in ChangeTracker.Entries<ITenantOwned>())
        {
            if (entry.State == EntityState.Added && string.IsNullOrEmpty(entry.Entity.TenantId))
            {
                entry.Entity.TenantId = _tenant.CurrentTenantId;
            }

            // Impede que uma entidade mude de dono por acidente.
            if (entry.State == EntityState.Modified && entry.Entity.TenantId != _tenant.CurrentTenantId)
            {
                throw new InvalidOperationException(
                    $"Tentativa de gravar registro do tenant '{entry.Entity.TenantId}' estando em '{_tenant.CurrentTenantId}'.");
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
