
using MyLibrary;

public class BookPublish
{
    public BookPublish()
    {
        Reservations = new HashSet<Reservation>();
    }

    public int Id { get; set; }
    public int BookFK { get; set; }
    public string DemirbasNo { get; set; }
    public int YayinEviFK { get; set; }

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public Book Book { get; set; }
    public YayinEvi YayinEvi { get; set; }
    public RentBook RentBook { get; set; }

    public ICollection<Reservation> Reservations { get; set; }
}
