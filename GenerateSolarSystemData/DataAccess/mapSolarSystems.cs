using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace QuesadillaExport.DataAccess
{
    public class mapSolarSystems : DataAccess
    {
        public static List<Models.Database.mapSolarSystems> SelectAll()
        {
            using (var conn = ConnectionFactory())
            {
                conn.Open();
                return conn.Query<Models.Database.mapSolarSystems>(@"SELECT mr.regionName, mss.* FROM mapSolarSystems mss, mapRegions mr WHERE mss.regionId < 11000000 AND mss.regionID = mr.regionID order by mss.regionID, mss.solarSystemID").ToList();
            }
        }
    }
}
