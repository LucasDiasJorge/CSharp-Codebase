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
    public class AuthorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthorsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors()
        {
            List<AuthorDto> authors = await _context.Authors
                .AsNoTracking()
                .Include(a => a.Books)
                .Select(a => new AuthorDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Books = a.Books
                        .Select(b => new AuthorBookDto { Id = b.Id, Title = b.Title })
                        .ToList()
                })
                .ToListAsync();

            return Ok(authors);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorDto>> GetAuthorById(int id)
        {
            AuthorDto? author = await _context.Authors
                .AsNoTracking()
                .Include(a => a.Books)
                .Where(a => a.Id == id)
                .Select(a => new AuthorDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Books = a.Books
                        .Select(b => new AuthorBookDto { Id = b.Id, Title = b.Title })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (author == null)
            {
                return NotFound();
            }

            return Ok(author);
        }

        [HttpPost]
        public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorDto authorDto)
        {
            if (string.IsNullOrWhiteSpace(authorDto.Name))
            {
                return BadRequest("Author name is required.");
            }

            Author author = new Author { Name = authorDto.Name };
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            AuthorDto result = new AuthorDto
            {
                Id = author.Id,
                Name = author.Name,
                Books = new List<AuthorBookDto>()
            };

            return CreatedAtAction(nameof(GetAuthorById), new { id = author.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorDto authorDto)
        {
            if (id != authorDto.Id)
            {
                return BadRequest();
            }

            Author? author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(authorDto.Name))
            {
                return BadRequest("Author name is required.");
            }

            author.Name = authorDto.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            Author? author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}