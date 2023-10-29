using System.Data;

namespace ElementSql.Interfaces
{
    public interface IConnectionSession
    {
        Task BeginSessionAsync();
        void EndSession();
        IDbConnection Connection { get; }
    }
}
