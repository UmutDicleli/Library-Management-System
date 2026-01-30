namespace MyLibrary
{
    public class RentBook
    {
        public int MemberFK { get; set; }
        public int BookPublishFK { get; set; }
        public Member Member { get; set; }
        public BookPublish BookPublish { get; set; }
        public string MemberFirstName { get; set; }
        public string MemberLastName { get; set; }
        public string DemirbasNo { get; set; }
    }
}
