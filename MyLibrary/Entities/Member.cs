using MyLibrary;

public class Member
{
    public Member()
    {
        RentedBooks = new HashSet<RentBook>();
        Reservations = new HashSet<Reservation>();
    }

    public int Id { get; set; }
    public string MemberFirstName { get; set; }
    public string MemberLastName { get; set; }
    public string MemberEmail { get; set; }
    public string MemberPhoneNumber { get; set; }

    public ICollection<RentBook> RentedBooks { get; set; }
    public ICollection<Reservation> Reservations { get; set; }
}
