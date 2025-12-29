using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace MyLibrary
{
    public class Author
    {
        public Author()
        {
            Kitaplar = new HashSet<Book>();
        }

        public int Id { get; set; }
        public string AuthorName { get; set; }
        public ICollection<Book> Kitaplar { get; set; }
    }
}