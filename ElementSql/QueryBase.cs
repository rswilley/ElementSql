using Dapper;
using ElementSql.Interfaces;
using System.Data;

namespace ElementSql
{
    //ExecuteAsync - impl
    //ExecuteReaderAsync - impl
    //ExecuteReaderSync
    //ExecuteScalarAsync - impl
    //QueryAsync - impl
    //QueryFirstAsync - impl
    //QueryFirstOrDefaultAsync - impl
    //QueryMultipleAsync - no?
    //QuerySingleAsync - impl
    //QuerySingleOrDefaultAsync - impl

    public abstract class QueryBase
    {
        public static async Task<T> ReadSingle<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QuerySingleAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<T?> ReadSingleOrDefault<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QuerySingleOrDefaultAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<T> ReadFirst<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QueryFirstAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<T?> ReadFirstOrDefault<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QueryFirstOrDefaultAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<IEnumerable<T>> ReadMany<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QueryAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<T?> ReadScalar<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.ExecuteScalarAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<IDataReader> ReadDataReader(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.ExecuteReaderAsync(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<int> Execute(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.ExecuteAsync(query, param, parts.Transaction, commandTimeout, commandType);
        }

        //public static async Task<GridReader> ReadMultiple(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        //{
        //    var parts = context.GetConnectionParts();
        //    return await parts.Connection.QueryMultipleAsync(query, param, parts.Transaction, commandTimeout, commandType);
        //}
    }
}
