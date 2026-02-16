namespace sqlite_sample_api.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int AuthorId { get; set; }

    public Author? Author { get; set; }
}