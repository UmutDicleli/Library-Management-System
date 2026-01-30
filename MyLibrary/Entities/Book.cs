namespace MyLibrary
{
    public class Book
    {
        public int Id { get; set; }
        public string KitapAdi { get; set; } = null!;
        public string ISBN { get; set; } = null!;

        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;

        public ICollection<BookPublish> BookPublishes { get; set; } = new List<BookPublish>();
    }
}
