using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary
{
    public class RentBook
    {
        public int MemberFK { get; set; }
        public int BookPublishFK { get; set; }

        public Member Member { get; set; }
        public BookPublish BookPublish { get; set; }
    }
}
