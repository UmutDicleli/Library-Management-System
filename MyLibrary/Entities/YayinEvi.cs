namespace MyLibrary
{
    public class YayinEvi
    {
        public YayinEvi()
        {
            BookPublishes = new HashSet<BookPublish>();
        }

        public int Id { get; set; }
        public string YayinEviName { get; set; }

        public string? PhotoUrl { get; set; }
        public string? PhotoHash { get; set; }

        public ICollection<BookPublish> BookPublishes { get; set; }
    }
}
