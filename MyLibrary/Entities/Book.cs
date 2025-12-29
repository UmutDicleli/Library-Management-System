using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary
{
    public class Book
    {
        public Book()
        {
            Publishes = new HashSet<BookPublish>();
        }
        public int Id { get; set; }
        public string KitapAdi { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; }
        public ICollection<BookPublish> Publishes { get; set; }
    }
}
