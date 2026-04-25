using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using sqlite_sample_api.Data;
using sqlite_sample_api.Models;

namespace sqlite_sample_api.Tests.Integration;

public class SqliteDatabaseIntegrationTests
{
    [Fact]
    public async Task SqliteRuntime_CreatesSchema_AndPersistsAssociations()
    {
        using SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        
        await using AppDbContext context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();
        
        Author author = new Author { Name = "Integration Author" };
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        context.Books.Add(new Book
        {
            Title = "Integration Book",
            Genre = "Sci-Fi",
            AuthorId = author.Id
        });
        await context.SaveChangesAsync();

        Book savedBook = await context.Books
            .Include(b => b.Author)
            .SingleAsync();

        Assert.Equal("Integration Book", savedBook.Title);
        Assert.NotNull(savedBook.Author);
        Assert.Equal("Integration Author", savedBook.Author!.Name);
    }

    [Fact]
    public async Task SqliteRuntime_CascadeDelete_RemovesBooksWhenAuthorIsDeleted()
    {
        using SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        
        await using AppDbContext context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();
        
        Author author = new Author { Name = "Delete Author" };
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        context.Books.Add(new Book { Title = "Delete Book", Genre = "Drama", AuthorId = author.Id });
        await context.SaveChangesAsync();

        context.Authors.Remove(author);
        await context.SaveChangesAsync();

        int remainingBooks = await context.Books.CountAsync();
        Assert.Equal(0, remainingBooks);
    }

    [Fact]
    public async Task DbInitializer_SeedsData_WhenDatabaseIsEmpty()
    {
        using SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        
        await using AppDbContext context = new AppDbContext(options);
        DbInitializer.Initialize(context);
        int authorsCount = await context.Authors.CountAsync();
        int booksCount = await context.Books.CountAsync();

        Assert.True(authorsCount >= 2);
        Assert.True(booksCount >= 3);
    }
}
