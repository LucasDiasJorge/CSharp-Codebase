using Microsoft.EntityFrameworkCore;
using sqlite_sample_api.Models;

namespace sqlite_sample_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>()
                .Property(a => a.Name)
                .HasMaxLength(120)
                .IsRequired();

            modelBuilder.Entity<Book>()
                .Property(b => b.Title)
                .HasMaxLength(160)
                .IsRequired();

            modelBuilder.Entity<Book>()
                .Property(b => b.Genre)
                .HasMaxLength(80)
                .IsRequired();

            modelBuilder.Entity<Author>()
                .HasMany(a => a.Books)
                .WithOne(b => b.Author)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}