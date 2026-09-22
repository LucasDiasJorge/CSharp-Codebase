using EfCoreRelationshipsDemo.Data;
using EfCoreRelationshipsDemo.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EfCoreRelationshipsDemo.Demo;

/// <summary>
/// Seis cenários: os três tipos de relacionamento, owned type, N+1 medido e tracking.
/// </summary>
public sealed class RelationshipsDemoRunner
{
    private readonly QueryCounter _counter;
    private readonly ILogger<RelationshipsDemoRunner> _logger;

    public RelationshipsDemoRunner(QueryCounter counter, ILogger<RelationshipsDemoRunner> logger)
    {
        _counter = counter;
        _logger = logger;
    }

    public async Task RunAllAsync()
    {
        await SeedAsync();
        await RunOneToManyAsync();
        await RunManyToManyAsync();
        await RunOneToOneAsync();
        await RunOwnedTypeAsync();
        await RunNPlusOneAsync();
        await RunTrackingAsync();
    }

    private async Task SeedAsync()
    {
        Section("0. Recriando o banco e populando");

        await using BlogDbContext database = new BlogDbContext(_counter);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        Tag csharp = new Tag { Name = "csharp" };
        Tag efcore = new Tag { Name = "efcore" };
        Tag arquitetura = new Tag { Name = "arquitetura" };

        Blog blog = new Blog
        {
            Name = "Blog de .NET",
            Posts =
            [
                new Post { Title = "Relacionamentos no EF Core", Tags = [csharp, efcore] },
                new Post { Title = "Carregamento preguicoso", Tags = [efcore] },
                new Post { Title = "Monolito modular", Tags = [arquitetura, csharp] }
            ]
        };

        Author author = new Author
        {
            Name = "Ana Souza",
            Address = new Address { Street = "Rua das Flores, 100", City = "Curitiba" },
            Profile = new AuthorProfile { Bio = "Escreve sobre .NET desde 2015." }
        };

        database.Blogs.Add(blog);
        database.Authors.Add(author);

        await database.SaveChangesAsync();

        _logger.LogInformation("Banco recriado com 1 blog, 3 posts, 3 tags e 1 autor.");
    }

    private async Task RunOneToManyAsync()
    {
        Section("1. Um-para-muitos: a FK fica no lado 'muitos'");

        await using BlogDbContext database = new BlogDbContext(_counter);

        Blog? blog = await database.Blogs
            .Include(item => item.Posts)
            .FirstOrDefaultAsync();

        _logger.LogInformation("Blog \"{Nome}\" com {Quantidade} post(s):", blog!.Name, blog.Posts.Count);
        foreach (Post post in blog.Posts)
        {
            _logger.LogInformation("  - {Titulo} (BlogId={BlogId})", post.Title, post.BlogId);
        }
    }

    private async Task RunManyToManyAsync()
    {
        Section("2. Muitos-para-muitos: skip navigation, sem entidade de juncao no codigo");

        await using BlogDbContext database = new BlogDbContext(_counter);

        List<Post> posts = await database.Posts
            .Include(post => post.Tags)
            .ToListAsync();

        foreach (Post post in posts)
        {
            _logger.LogInformation("  {Titulo}: [{Tags}]", post.Title, string.Join(", ", post.Tags.Select(tag => tag.Name)));
        }

        Tag? efcore = await database.Tags
            .Include(tag => tag.Posts)
            .FirstOrDefaultAsync(tag => tag.Name == "efcore");

        _logger.LogInformation(
            "A navegacao vale nos dois sentidos: a tag \"{Tag}\" aparece em {Quantidade} post(s). A tabela PostTags existe no banco, mas nao no modelo.",
            efcore!.Name,
            efcore.Posts.Count);
    }

    private async Task RunOneToOneAsync()
    {
        Section("3. Um-para-um: quem tem a FK e o dependente");

        await using BlogDbContext database = new BlogDbContext(_counter);

        Author? author = await database.Authors
            .Include(item => item.Profile)
            .FirstOrDefaultAsync();

        _logger.LogInformation("{Autor}: {Bio}", author!.Name, author.Profile!.Bio);
        _logger.LogInformation(
            "AuthorProfile.AuthorId = {AutorId}. O perfil nao existe sem o autor; o autor existe sem perfil.",
            author.Profile.AuthorId);
    }

    private async Task RunOwnedTypeAsync()
    {
        Section("4. Owned type: colunas na tabela do dono, sem tabela propria");

        await using BlogDbContext database = new BlogDbContext(_counter);

        Author? author = await database.Authors.FirstOrDefaultAsync();

        _logger.LogInformation("Endereco de {Autor}: {Rua}, {Cidade}", author!.Name, author.Address.Street, author.Address.City);

        // Prova de que nao ha tabela separada: as colunas estao em Authors.
        List<string> columns = await database.Database
            .SqlQueryRaw<string>("SELECT name AS Value FROM pragma_table_info('Authors')")
            .ToListAsync();

        _logger.LogInformation("Colunas da tabela Authors: [{Colunas}]", string.Join(", ", columns));

        List<string> tables = await database.Database
            .SqlQueryRaw<string>("SELECT name AS Value FROM sqlite_master WHERE type='table' ORDER BY name")
            .ToListAsync();

        _logger.LogInformation("Tabelas no banco: [{Tabelas}] — nao ha tabela Address.", string.Join(", ", tables));
    }

    private async Task RunNPlusOneAsync()
    {
        Section("5. N+1: o custo de carregar em partes, medido");

        // SEM Include: uma consulta para os posts e uma por post para as tags.
        await using (BlogDbContext lazy = new BlogDbContext(_counter))
        {
            _counter.Reset();

            List<Post> posts = await lazy.Posts.ToListAsync();
            foreach (Post post in posts)
            {
                // Carregamento explicito, um por vez: e este laco que gera o N.
                await lazy.Entry(post).Collection(item => item.Tags).LoadAsync();
            }

            _logger.LogWarning(
                "Carregando em partes: {Consultas} consultas para {Posts} posts (1 + N).",
                _counter.Count,
                posts.Count);
        }

        // COM Include: o EF resolve tudo de uma vez.
        await using (BlogDbContext eager = new BlogDbContext(_counter))
        {
            _counter.Reset();

            List<Post> posts = await eager.Posts.Include(post => post.Tags).ToListAsync();

            _logger.LogInformation(
                "Com Include: {Consultas} consulta(s) para os mesmos {Posts} posts.",
                _counter.Count,
                posts.Count);
        }
    }

    private async Task RunTrackingAsync()
    {
        Section("6. Tracking: o que o contexto guarda, e o que custa");

        await using BlogDbContext tracked = new BlogDbContext(_counter);

        Post? post = await tracked.Posts.FirstAsync();
        post.Title = "titulo alterado em memoria";

        _logger.LogInformation(
            "Com tracking: EF detectou {Quantidade} entidade(s) modificada(s) sem que ninguem avisasse.",
            tracked.ChangeTracker.Entries().Count(entry => entry.State == EntityState.Modified));

        await using BlogDbContext untracked = new BlogDbContext(_counter);

        Post? readOnly = await untracked.Posts.AsNoTracking().FirstAsync();
        readOnly.Title = "alteracao que nao sera detectada";

        _logger.LogInformation(
            "Com AsNoTracking: {Quantidade} entidade(s) rastreada(s). A alteracao existe no objeto, mas SaveChanges nao gravaria nada.",
            untracked.ChangeTracker.Entries().Count());

        _logger.LogInformation("Use AsNoTracking em leitura pura: menos memoria e sem o custo de detectar mudancas.");
    }

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
