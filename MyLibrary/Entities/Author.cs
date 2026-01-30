namespace MyLibrary
{
    public class Author
    {
        public Author()
        {
            Kitaplar = new HashSet<Book>();
        }

        public int Id { get; set; }
        public string AuthorName { get; set; }

        public string? PhotoUrl { get; set; }
        public string? PhotoHash { get; set; }

        public ICollection<Book> Kitaplar { get; set; }
    }
}
