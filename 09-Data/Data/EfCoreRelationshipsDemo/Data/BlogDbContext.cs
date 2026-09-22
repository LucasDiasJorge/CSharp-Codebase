using EfCoreRelationshipsDemo.Model;
using Microsoft.EntityFrameworkCore;

namespace EfCoreRelationshipsDemo.Data;

public sealed class BlogDbContext : DbContext
{
    private readonly QueryCounter _counter;

    public BlogDbContext(QueryCounter counter)
    {
        _counter = counter;
    }

    public DbSet<Blog> Blogs => Set<Blog>();

    public DbSet<Post> Posts => Set<Post>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<Author> Authors => Set<Author>();

    public DbSet<AuthorProfile> AuthorProfiles => Set<AuthorProfile>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlite("Data Source=relationships-demo.db")
            .AddInterceptors(_counter);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Um-para-muitos. DeleteBehavior.Cascade e o padrao para relacao obrigatoria:
        // apagar o blog apaga os posts. Para relacao opcional o padrao e SetNull, e a
        // diferenca costuma so aparecer no primeiro delete em producao.
        modelBuilder.Entity<Blog>()
            .HasMany(blog => blog.Posts)
            .WithOne(post => post.Blog)
            .HasForeignKey(post => post.BlogId)
            .OnDelete(DeleteBehavior.Cascade);

        // Muitos-para-muitos por skip navigation. O EF cria a tabela de juncao sozinho;
        // ela so precisa ser declarada se tiver colunas proprias (data, quantidade...).
        modelBuilder.Entity<Post>()
            .HasMany(post => post.Tags)
            .WithMany(tag => tag.Posts)
            .UsingEntity(join => join.ToTable("PostTags"));

        // Um-para-um: o perfil e o dependente, porque carrega a FK.
        modelBuilder.Entity<Author>()
            .HasOne(author => author.Profile)
            .WithOne(profile => profile.Author)
            .HasForeignKey<AuthorProfile>(profile => profile.AuthorId);

        // Owned type: Address vira colunas em Authors, sem tabela propria.
        modelBuilder.Entity<Author>().OwnsOne(author => author.Address);
    }
}
