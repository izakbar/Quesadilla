using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes
{
    public class ItemType
    {
        public int Id { get; set; }
        public string Name { get; set; }
		public int GroupId { get; set; }
		public string Group { get; set; }
    }
}
