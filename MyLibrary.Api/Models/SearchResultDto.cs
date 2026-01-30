namespace MyLibrary.Api.Models
{
    public class SearchResultDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int TotalCount { get; set; }
    }
}
