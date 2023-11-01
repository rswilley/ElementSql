using ElementSql.Interfaces;
using System.Data;

namespace ElementSql
{
    public class SqlDatabase : ISqlDatabase
    {
        public Func<IDbConnection> DbConnection { get; set; } = null!;
        public string Name { get; set; } = "Default";
    }
}
