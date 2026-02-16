namespace sqlite_sample_api.Dtos
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public BookAuthorDto? Author { get; set; }
    }

    public class BookAuthorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}