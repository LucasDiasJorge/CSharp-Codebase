using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sqlite_sample_api.Data;
using sqlite_sample_api.Dtos;
using sqlite_sample_api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sqlite_sample_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
        {
            List<BookDto> books = await _context.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Genre = b.Genre,
                    AuthorId = b.AuthorId,
                    Author = b.Author == null
                        ? null
                        : new BookAuthorDto
                        {
                            Id = b.Author.Id,
                            Name = b.Author.Name
                        }
                })
                .ToListAsync();

            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBookById(int id)
        {
            BookDto? book = await _context.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .Where(b => b.Id == id)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Genre = b.Genre,
                    AuthorId = b.AuthorId,
                    Author = b.Author == null
                        ? null
                        : new BookAuthorDto
                        {
                            Id = b.Author.Id,
                            Name = b.Author.Name
                        }
                })
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<BookDto>> CreateBook(BookDto bookDto)
        {
            if (string.IsNullOrWhiteSpace(bookDto.Title) || string.IsNullOrWhiteSpace(bookDto.Genre))
            {
                return BadRequest("Title and genre are required.");
            }

            bool authorExists = await _context.Authors.AnyAsync(a => a.Id == bookDto.AuthorId);
            if (!authorExists)
            {
                return BadRequest("AuthorId does not exist.");
            }

            Book book = new Book
            {
                Title = bookDto.Title,
                Genre = bookDto.Genre,
                AuthorId = bookDto.AuthorId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            BookDto createdBook = await _context.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .Where(b => b.Id == book.Id)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Genre = b.Genre,
                    AuthorId = b.AuthorId,
                    Author = b.Author == null
                        ? null
                        : new BookAuthorDto
                        {
                            Id = b.Author.Id,
                            Name = b.Author.Name
                        }
                })
                .FirstAsync();

            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, createdBook);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookDto bookDto)
        {
            if (id != bookDto.Id)
            {
                return BadRequest();
            }

            Book? book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(bookDto.Title) || string.IsNullOrWhiteSpace(bookDto.Genre))
            {
                return BadRequest("Title and genre are required.");
            }

            bool authorExists = await _context.Authors.AnyAsync(a => a.Id == bookDto.AuthorId);
            if (!authorExists)
            {
                return BadRequest("AuthorId does not exist.");
            }

            book.Title = bookDto.Title;
            book.Genre = bookDto.Genre;
            book.AuthorId = bookDto.AuthorId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            Book? book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}