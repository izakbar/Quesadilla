using System.Data.Common;
using MySql.Data.MySqlClient;

namespace QuesadillaExport.DataAccess
{
    public class DataAccess
    {
        public static string ConnectionString = "";
        public static Func<DbConnection> ConnectionFactory
        {
            get { return () => new MySqlConnection(ConnectionString); }
        }
    }
}
