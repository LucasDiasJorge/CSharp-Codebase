using Microsoft.EntityFrameworkCore;
using TransactionalInboxDemo.Domain;

namespace TransactionalInboxDemo.Inbox;

/// <summary>
/// Inbox e domínio no MESMO banco e no mesmo contexto. É a condição do padrão: só
/// assim a marca "já processei" e o efeito no domínio entram na mesma transação.
///
/// Inbox em outro banco (ou em Redis) reintroduz o dual write — haveria um instante em
/// que um dos dois existe sem o outro.
/// </summary>
public sealed class InboxDbContext : DbContext
{
    public InboxDbContext(DbContextOptions<InboxDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(account => account.Id);
            entity.HasIndex(account => account.AccountId).IsUnique();
        });

        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.HasKey(message => message.Id);

            // O indice UNICO e a garantia de verdade. A consulta previa resolve o caso
            // comum; duas entregas simultaneas da mesma mensagem passariam pelas duas
            // consultas e so este indice impede que as duas gravem.
            entity.HasIndex(message => message.MessageId).IsUnique();
        });
    }
}
