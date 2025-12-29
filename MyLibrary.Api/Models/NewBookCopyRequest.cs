namespace MyLibrary.Api.Models
{
    public class NewBookCopyRequest
    {
        public string BookTitle { get; set; }
        public string AuthorName { get; set; }
        public string PublisherName { get; set; }
    }
}