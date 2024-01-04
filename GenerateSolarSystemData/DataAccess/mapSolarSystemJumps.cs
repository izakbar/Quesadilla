using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace QuesadillaExport.DataAccess
{
    public class mapSolarSystemJumps : DataAccess
    {
        public static List<Models.Database.mapSolarSystemJumps> SelectAll()
        {
            using (var conn = ConnectionFactory())
            {
                conn.Open();
                return conn.Query<Models.Database.mapSolarSystemJumps>(@"SELECT * FROM mapSolarSystemJumps").ToList();
            }
        }
    }
}
