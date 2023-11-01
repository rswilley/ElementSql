using System.Data;

namespace ElementSql.Interfaces
{
    internal interface ISqlDatabase
    {
        Func<IDbConnection> DbConnection { get; set; }
        string Name { get; set; }
    }
}
