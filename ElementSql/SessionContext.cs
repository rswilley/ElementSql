using ElementSql.Interfaces;
using System.Data;

namespace ElementSql
{
    internal class SessionContext : ISessionContext
    {
        public IDbConnection Connection { get; }

        public bool WasSuccessful { get; set; }

        public SessionContext(IConnectionSession session)
        {
            _session = session;
            Connection = session.Connection;
        }

        public async Task<TEntity> InsertAsync<TEntity>(TEntity entity, int? commandTimeout = null) where TEntity : EntityBase
        {
            return await EntityHelper.InsertAsync(entity, this, commandTimeout);
        }

        public Task<TEntity?> FindAsync<TEntity>(int? commandTimeout = null) where TEntity : EntityBase
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync<TEntity>(TEntity entity, int? commandTimeout = null) where TEntity : EntityBase
        {
            await EntityHelper.UpdateAsync(entity, this, commandTimeout);
        }

        public async Task DeleteAsync<T>(T entity, int? commandTimeout = null) where T : EntityBase
        {
            await EntityHelper.DeleteAsync(entity, this, commandTimeout);
        }

        public Task<TResult> QueryFirstAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            throw new NotImplementedException();
        }

        public Task<TResult?> QueryFirstOrDefaultAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            throw new NotImplementedException();
        }

        public Task<TResult> QuerySingleAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            throw new NotImplementedException();
        }

        public Task<TResult?> QuerySingleOrDefaultAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> QueryAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            throw new NotImplementedException();
        }

        public Task<TResult?> ExecuteScalarAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            throw new NotImplementedException();
        }

        public Task<IDataReader> ExecuteReaderAsync(IQuery query, IConnectionContext context, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> ExecuteAsync(IQuery query, IConnectionContext context, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _session.EndSession();
        }

        private readonly IConnectionSession _session;
    }
}
