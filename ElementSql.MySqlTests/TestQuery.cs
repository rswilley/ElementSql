using ElementSql.Interfaces;

namespace ElementSql.MySqlTests
{
    internal interface ITestQuery : ISqlQuery
    {
        Task<DateTime> GetCurrentTime(IConnectionContext context);
    }

    internal class TestQuery : QueryBase, ITestQuery
    {
        public async Task<DateTime> GetCurrentTime(IConnectionContext context)
        {
            return await ReadScalarAsync<DateTime>("SELECT UTC_TIMESTAMP()", null!, context);
        }
    }
}
