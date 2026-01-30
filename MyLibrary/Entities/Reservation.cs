using MyLibrary;

public class Reservation
{
    public int Id { get; set; }

    public int MemberFK { get; set; }
    public Member Member { get; set; }

    public int BookPublishFK { get; set; }
    public BookPublish BookPublish { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
}
