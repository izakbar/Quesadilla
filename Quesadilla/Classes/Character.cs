using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes
{
    public class Character
    {
        public string CharacterName {  get; set; }
        public string Location { get; set; }
        public int LocationId { get; set; }
        public bool IsClientActive { get; set; }
    }
}
