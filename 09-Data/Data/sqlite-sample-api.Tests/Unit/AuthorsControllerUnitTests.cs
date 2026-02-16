using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sqlite_sample_api.Controllers;
using sqlite_sample_api.Data;
using sqlite_sample_api.Dtos;
using sqlite_sample_api.Models;

namespace sqlite_sample_api.Tests.Unit;

public class AuthorsControllerUnitTests
{
    [Fact]
    public async Task GetAuthors_ReturnsAuthorWithAssociatedBooks()
    {
        using AppDbContext context = CreateInMemoryContext();

        Author author = new Author { Name = "Author A" };
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        context.Books.Add(new Book { Title = "Book A", Genre = "Tech", AuthorId = author.Id });
        await context.SaveChangesAsync();

        AuthorsController controller = new AuthorsController(context);

        ActionResult<IEnumerable<AuthorDto>> result = await controller.GetAuthors();

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        IEnumerable<AuthorDto> authors = Assert.IsAssignableFrom<IEnumerable<AuthorDto>>(ok.Value);
        AuthorDto first = Assert.Single(authors);
        Assert.Equal("Author A", first.Name);
        Assert.Single(first.Books);
        Assert.Equal("Book A", first.Books[0].Title);
    }

    [Fact]
    public async Task CreateAuthor_WithEmptyName_ReturnsBadRequest()
    {
        using AppDbContext context = CreateInMemoryContext();
        AuthorsController controller = new AuthorsController(context);

        ActionResult<AuthorDto> result = await controller.CreateAuthor(new AuthorDto { Name = " " });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    private static AppDbContext CreateInMemoryContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
