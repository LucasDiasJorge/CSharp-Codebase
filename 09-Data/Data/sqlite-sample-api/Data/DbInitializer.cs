using System.Linq;
using sqlite_sample_api.Models;

namespace sqlite_sample_api.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any authors.
            if (context.Authors.Any())
            {
                return;   // DB has been seeded
            }

            Author[] authors = new Author[]
            {
                new Author { Name = "George Orwell" },
                new Author { Name = "Clarice Lispector" }
            };

            foreach (Author author in authors)
            {
                context.Authors.Add(author);
            }

            context.SaveChanges();

            Book[] books = new Book[]
            {
                new Book { Title = "1984", Genre = "Dystopian", AuthorId = authors[0].Id },
                new Book { Title = "A Hora da Estrela", Genre = "Novel", AuthorId = authors[1].Id },
                new Book { Title = "Animal Farm", Genre = "Political satire", AuthorId = authors[0].Id }
            };

            foreach (Book book in books)
            {
                context.Books.Add(book);
            }

            context.SaveChanges();
        }
    }
}