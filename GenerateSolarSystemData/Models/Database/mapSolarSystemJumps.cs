using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuesadillaExport.Models.Database
{
    public class mapSolarSystemJumps
    {
        public int fromRegionId { get; set; }
        public int toRegionId { get; set; }
        public int fromSolarSystemId { get; set; }
        public int toSolarSystemId { get; set; }
    }
}
