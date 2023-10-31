using Dapper;
using ElementSql.Interfaces;
using System.Data;

namespace ElementSql
{
    public abstract class QueryBase
    {
        public static async Task<TEntity?> ReadSingleOrDefault<TEntity>(string query, object param, IConnectionContext context) where TEntity : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QuerySingleOrDefaultAsync<TEntity>(query, param, parts.Transaction);
        }

        public static async Task<TEntity?> ReadFirstOrDefault<TEntity>(string query, object param, IConnectionContext context) where TEntity : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QueryFirstOrDefaultAsync<TEntity>(query, param, parts.Transaction);
        }

        public static async Task<IEnumerable<TEntity>> ReadMany<TEntity>(string query, object param, IConnectionContext context) where TEntity : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QueryAsync<TEntity>(query, param, parts.Transaction);
        }

        public static async Task<TEntity?> ReadScalar<TEntity>(string query, object param, IConnectionContext context)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.ExecuteScalarAsync<TEntity>(query, param, parts.Transaction);
        }

        public static async Task<IDataReader> ReadDataReader(string query, object param, IConnectionContext context)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.ExecuteReaderAsync(query, param, parts.Transaction);
        }
    }

    public abstract class CommandBase
    {

    }
}
