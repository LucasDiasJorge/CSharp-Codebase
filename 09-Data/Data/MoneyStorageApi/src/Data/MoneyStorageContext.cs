using Microsoft.EntityFrameworkCore;
using MoneyStorageApi.Domain;

namespace MoneyStorageApi.Data;

public class MoneyStorageContext : DbContext
{
    public MoneyStorageContext(DbContextOptions<MoneyStorageContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<MoneyMovement> MoneyMovements => Set<MoneyMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var account = modelBuilder.Entity<Account>();
        account.ToTable("accounts");
        account.HasKey(a => a.Id);
        account.Property(a => a.OwnerName)
            .HasMaxLength(120)
            .IsRequired();
        account.Property(a => a.Balance)
            .HasColumnType("decimal(18,2)");
        account.Property(a => a.CreatedAtUtc)
            .HasColumnType("datetime(6)");
        account.Property(a => a.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
        account.HasMany(a => a.Movements)
            .WithOne()
            .HasForeignKey(m => m.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        var movement = modelBuilder.Entity<MoneyMovement>();
        movement.ToTable("money_movements");
        movement.HasKey(m => m.Id);
        movement.Property(m => m.Type)
            .HasConversion<int>();
        movement.Property(m => m.Amount)
            .HasColumnType("decimal(18,2)");
        movement.Property(m => m.Description)
            .HasMaxLength(200);
        movement.Property(m => m.OccurredAtUtc)
            .HasColumnType("datetime(6)");
    }
}
