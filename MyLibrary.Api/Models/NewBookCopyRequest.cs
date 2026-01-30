public class NewBookCopiesRequest
{
    public string BookTitle { get; set; }
    public string AuthorName { get; set; }
    public string PublisherName { get; set; }
    public string ISBN { get; set; }

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public string? AuthorPhotoUrl { get; set; }
    public string? PublisherPhotoUrl { get; set; }

    public int Quantity { get; set; }
    public string DemirbasPrefix { get; set; }
    public int StartNumber { get; set; }
}
