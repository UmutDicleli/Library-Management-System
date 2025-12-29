using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary
{
    public class YayinEvi
    {
        public YayinEvi()
        {
            BookPublishes = new HashSet<BookPublish>();
        }

        public int Id { get; set; }
        public string YayinEviName { get; set; }

        public ICollection<BookPublish> BookPublishes { get; set; }
    }
}
