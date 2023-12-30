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
                return conn.Query<Models.Database.mapSolarSystems>(@"SELECT * FROM mapSolarSystems WHERE regionId < 11000000").ToList();
            }
        }
    }
}
