using Dapper;
using ElementSql.Interfaces;
using System.Data;

namespace ElementSql
{
    public abstract class QueryBase
    {
        public static async Task<T> ReadSingleAsync<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QuerySingleAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<T?> ReadSingleOrDefaultAsync<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QuerySingleOrDefaultAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<T> ReadFirstAsync<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QueryFirstAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<T?> ReadFirstOrDefaultAsync<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QueryFirstOrDefaultAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<IEnumerable<T>> ReadManyAsync<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QueryAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<T?> ReadScalarAsync<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.ExecuteScalarAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<IDataReader> ReadDataReaderAsync(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.ExecuteReaderAsync(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static async Task<int> ExecuteAsync(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.ExecuteAsync(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static T ReadSingle<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.QuerySingle<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static T? ReadSingleOrDefault<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.QuerySingleOrDefault<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static T ReadFirst<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.QueryFirst<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static T? ReadFirstOrDefault<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.QueryFirstOrDefault<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static IEnumerable<T> ReadMany<T>(string query, object param, IConnectionContext context, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.Query<T>(query, param, parts.Transaction, buffered, commandTimeout, commandType);
        }

        public static T? ReadScalar<T>(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.ExecuteScalar<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static IDataReader ReadDataReader(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.ExecuteReader(query, param, parts.Transaction, commandTimeout, commandType);
        }

        public static int Execute(string query, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.Execute(query, param, parts.Transaction, commandTimeout, commandType);
        }
    }
}
