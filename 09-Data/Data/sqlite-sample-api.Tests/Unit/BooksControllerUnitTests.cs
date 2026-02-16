using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sqlite_sample_api.Controllers;
using sqlite_sample_api.Data;
using sqlite_sample_api.Dtos;
using sqlite_sample_api.Models;

namespace sqlite_sample_api.Tests.Unit;

public class BooksControllerUnitTests
{
    [Fact]
    public async Task CreateBook_WithUnknownAuthor_ReturnsBadRequest()
    {
        using AppDbContext context = CreateInMemoryContext();
        BooksController controller = new BooksController(context);

        ActionResult<BookDto> result = await controller.CreateBook(new BookDto
        {
            Title = "Book X",
            Genre = "Fantasy",
            AuthorId = 999
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetBookById_WhenNotFound_ReturnsNotFound()
    {
        using AppDbContext context = CreateInMemoryContext();
        BooksController controller = new BooksController(context);

        ActionResult<BookDto> result = await controller.GetBookById(404);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateBook_WithValidAuthor_ReturnsCreatedWithAuthorData()
    {
        using AppDbContext context = CreateInMemoryContext();

        Author author = new Author { Name = "Author Z" };
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        BooksController controller = new BooksController(context);

        ActionResult<BookDto> result = await controller.CreateBook(new BookDto
        {
            Title = "Book Z",
            Genre = "Drama",
            AuthorId = author.Id
        });
        CreatedAtActionResult created = Assert.IsType<CreatedAtActionResult>(result.Result);
        BookDto dto = Assert.IsType<BookDto>(created.Value);
        Assert.Equal("Book Z", dto.Title);
        Assert.NotNull(dto.Author);
        Assert.Equal("Author Z", dto.Author!.Name);
    }

    private static AppDbContext CreateInMemoryContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
