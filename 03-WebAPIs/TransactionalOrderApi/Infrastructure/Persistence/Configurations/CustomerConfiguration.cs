using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionalOrderApi.Domain.Entities;

namespace TransactionalOrderApi.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FullName).HasMaxLength(180).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(180).IsRequired();
        builder.HasIndex(c => c.Email).IsUnique();
        builder.Property(c => c.TotalSpent).HasPrecision(18, 2);
    }
}
