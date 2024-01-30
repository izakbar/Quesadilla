using Dapper;
using Quesadilla.Classes;
using System.Collections.Generic;
using System.Linq;

namespace QuesadillaExport.DataAccess
{
    public class invTypes : DataAccess
    {
        public static List<Models.Database.invType> SelectAll()
        {
            using (var conn = ConnectionFactory())
            {
                conn.Open();
                return conn.Query<Models.Database.invType>(@"SELECT groupName, typename, typeId FROM invGroups ig, invTypes it WHERE it.groupID = ig.groupID ORDER BY typeId").ToList();
            }
        }
    }
}
