namespace sqlite_sample_api.Dtos
{
    public class AuthorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<AuthorBookDto> Books { get; set; } = new();
    }

    public class AuthorBookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}