using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary
{
    public class Member
    {
        public Member()
        {
            RentedBooks = new HashSet<RentBook>();
        }
        public int Id { get; set; }
        public string MemberFirstName { get; set; }
        public string MemberLastName { get; set; }
        public string MemberEmail { get; set; }
        public string MemberPhoneNumber { get; set; }

        public ICollection<RentBook> RentedBooks { get; set; }
    }
}
