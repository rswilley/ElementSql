using System.Data;

namespace ElementSql.Interfaces
{
    public interface IConnectionContext : IDisposable
    {
        Task<TEntity> InsertAsync<TEntity>(TEntity entity, int? commandTimeout = null) where TEntity : EntityBase;
        Task<TEntity?> FindAsync<TEntity>(int? commandTimeout = null) where TEntity : EntityBase;
        Task UpdateAsync<TEntity>(TEntity entity, int? commandTimeout = null) where TEntity : EntityBase;
        Task DeleteAsync<T>(T entity, int? commandTimeout = null) where T : EntityBase;

        Task<TResult> QueryFirstAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : QueryBase;
        Task<TResult?> QueryFirstOrDefaultAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : QueryBase;

        Task<TResult> QuerySingleAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : QueryBase;
        Task<TResult?> QuerySingleOrDefaultAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : QueryBase;

        Task<IEnumerable<TResult>> QueryAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : QueryBase;
        Task<TResult?> ExecuteScalarAsync<TResult>(IQuery query, int? commandTimeout = null, CommandType? commandType = null) where TResult : QueryBase;

        Task<IDataReader> ExecuteReaderAsync(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null);

        Task<int> ExecuteAsync(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null);

        bool WasSuccessful { get; set; }
    }
}
