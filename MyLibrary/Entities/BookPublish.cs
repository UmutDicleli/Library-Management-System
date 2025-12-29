using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary
{
    public class BookPublish
    {
        public int Id { get; set; }
        public int BookFK { get; set; }
        public Book Book { get; set; }
        public int YayinEviFK { get; set; }
        public YayinEvi YayinEvi { get; set; }
        public RentBook RentBook { get; set; }
    }
}
