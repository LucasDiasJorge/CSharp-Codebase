using Microsoft.EntityFrameworkCore;
using TransactionalOutboxDemo.Domain;

namespace TransactionalOutboxDemo.Outbox;

/// <summary>
/// Pedidos e outbox no MESMO contexto e no mesmo banco. Não é detalhe de organização:
/// é a condição para que um único <c>SaveChangesAsync</c> grave os dois atomicamente.
/// Outbox em outro banco reintroduz exatamente o dual write que o padrão evita.
/// </summary>
public sealed class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(message => message.Id);
            entity.HasIndex(message => message.MessageId).IsUnique();

            // Índice sobre o que o relay consulta a cada ciclo: as pendentes, em ordem.
            entity.HasIndex(message => new { message.PublishedAt, message.Id });
        });

        modelBuilder.Entity<Order>().HasKey(order => order.Id);
    }
}
