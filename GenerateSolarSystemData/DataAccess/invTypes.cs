using Dapper;
using Quesadilla.Classes;
using System.Collections.Generic;
using System.Linq;

namespace QuesadillaExport.DataAccess
{
    public class invTypes : DataAccess
    {
        public static List<Models.Database.invType> SelectAll(int category)
        {
            using (var conn = ConnectionFactory())
            {
                conn.Open();
                return conn.Query<Models.Database.invType>(@"SELECT groupName, typename, typeId FROM invGroups ig, invTypes it WHERE ig.categoryId = @cat AND it.groupID = ig.groupID ORDER BY typeId",
                    new { cat = category }).ToList();
            }
        }
    }
}
