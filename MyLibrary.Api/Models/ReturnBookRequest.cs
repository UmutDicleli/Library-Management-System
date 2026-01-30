namespace MyLibrary.Api.Models
{
    public class ReturnBookRequest
    {
        public string MemberEmail { get; set; }
        public int DemirbasNo { get; set; }
        public string DemirbasPrefix { get; set; }
    }
}
