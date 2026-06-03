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

        public async Task<TEntity> InsertAsync<TEntity>(TEntity entity, int? commandTimeout = null) where TEntity : IEntityRecordBase
        {
            return await EntityHelper.InsertAsync(entity, this, commandTimeout);
        }

        public async Task<TEntity?> GetByIdAsync<TEntity>(object id, int? commandTimeout = null) where TEntity : IEntityRecordBase
        {
            return await EntityHelper.GetByIdAsync<TEntity>(id, this, commandTimeout);
        }

        public Task<TEntity?> FindAsync<TEntity>(int? commandTimeout = null) where TEntity : IEntityRecordBase
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync<TEntity>(TEntity entity, int? commandTimeout = null) where TEntity : IEntityRecordBase
        {
            await EntityHelper.UpdateAsync(entity, this, commandTimeout);
        }

        public async Task DeleteAsync<TEntity>(TEntity entity, int? commandTimeout = null) where TEntity : IEntityRecordBase
        {
            await EntityHelper.DeleteAsync(entity, this, commandTimeout);
        }

        public Task<TResult> QueryFirstAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            return QueryHelper.QueryFirstAsync<TResult>(query, this, commandTimeout, commandType);
        }

        public Task<TResult?> QueryFirstOrDefaultAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            return QueryHelper.QueryFirstOrDefaultAsync<TResult>(query, this, commandTimeout, commandType);
        }

        public Task<TResult> QuerySingleAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            return QueryHelper.QuerySingleAsync<TResult>(query, this, commandTimeout, commandType);
        }

        public Task<TResult?> QuerySingleOrDefaultAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            return QueryHelper.QuerySingleOrDefaultAsync<TResult>(query, this, commandTimeout, commandType);
        }

        public Task<IEnumerable<TResult>> QueryAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            return QueryHelper.QueryAsync<TResult>(query, this, commandTimeout, commandType);
        }

        public Task<TResult?> ExecuteScalarAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
        {
            return QueryHelper.ExecuteScalarAsync<TResult>(query, this, commandTimeout, commandType);
        }

        public Task<IDataReader> ExecuteReaderAsync(IQuery query, IConnectionContext context, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return QueryHelper.ExecuteReaderAsync(query, this, commandTimeout, commandType);
        }

        public Task<int> ExecuteAsync(IQuery query, IConnectionContext context, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return QueryHelper.ExecuteAsync(query, this, commandTimeout, commandType);
        }

        public void Dispose()
        {
            _session.EndSession();
        }

        private readonly IConnectionSession _session;
    }
}
