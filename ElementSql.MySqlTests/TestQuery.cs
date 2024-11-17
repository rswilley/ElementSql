using ElementSql.Interfaces;

namespace ElementSql.MySqlTests
{
    public interface ITestQuery : IElementSqlQuery
    {
        Task<DateTime> GetCurrentTime(IConnectionContext context);
    }

    public class TestQuery : QueryBase, ITestQuery
    {
        public async Task<DateTime> GetCurrentTime(IConnectionContext context)
        {
            return await ExecuteScalarAsync<DateTime>("SELECT UTC_TIMESTAMP()", null!, context);
        }
    }
}
