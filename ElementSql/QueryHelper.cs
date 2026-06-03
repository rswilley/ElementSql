using System.Data;
using Dapper;
using ElementSql.Interfaces;

namespace ElementSql;

internal static class QueryHelper
{
    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="TResult">The type of result to return.</typeparam>
    /// <param name="query">The SQL to execute for the query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<TResult> QuerySingleAsync<TResult>(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
    {
        var parts = context.GetConnectionParts();
        return await parts.Connection.QuerySingleAsync<TResult>(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="TResult">The type to return.</typeparam>
    /// <param name="query">The SQL to execute for the query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<TResult?> QuerySingleOrDefaultAsync<TResult>(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
    {
        var parts = context.GetConnectionParts();
        return await parts.Connection.QuerySingleOrDefaultAsync<TResult>(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="TResult">The type of result to return.</typeparam>
    /// <param name="query">The SQL to execute for the query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<TResult> QueryFirstAsync<TResult>(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
    {
        var parts = context.GetConnectionParts();
        return await parts.Connection.QueryFirstAsync<TResult>(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="TResult">The type of result to return.</typeparam>
    /// <param name="query">The SQL to execute for the query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<TResult?> QueryFirstOrDefaultAsync<TResult>(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
    {
        var parts = context.GetConnectionParts();
        return await parts.Connection.QueryFirstOrDefaultAsync<TResult>(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute a query asynchronously using Task.
    /// </summary>
    /// <typeparam name="TResult">The type of results to return.</typeparam>
    /// <param name="query">The SQL to execute for the query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="TResult"/>; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static async Task<IEnumerable<TResult>> QueryAsync<TResult>(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
    {
        var parts = context.GetConnectionParts();
        return await parts.Connection.QueryAsync<TResult>(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute parameterized SQL that selects a single value.
    /// </summary>
    /// <typeparam name="TResult">The type to return.</typeparam>
    /// <param name="query">The SQL to execute.</param>
    /// <param name="context">The connection context.</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The first cell returned, as <typeparamref name="TResult"/>.</returns>
    public static async Task<TResult?> ExecuteScalarAsync<TResult>(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        return await parts.Connection.ExecuteScalarAsync<TResult>(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute parameterized SQL and return an <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="query">The SQL to execute.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>An <see cref="IDataReader"/> that can be used to iterate over the results of the SQL query.</returns>
    /// <remarks>
    /// This is typically used when the results of a query are not processed by Dapper, for example, used to fill a <see cref="DataTable"/>
    /// or <see cref="T:DataSet"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// DataTable table = new DataTable("MyTable");
    /// using (var reader = ExecuteReader(cnn, sql, param))
    /// {
    ///     table.Load(reader);
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public static async Task<IDataReader> ExecuteReaderAsync(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        return await parts.Connection.ExecuteReaderAsync(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute a command asynchronously using Task.
    /// </summary>
    /// <param name="query">The SQL to execute for this query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The number of rows affected.</returns>
    public static async Task<int> ExecuteAsync(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        return await parts.Connection.ExecuteAsync(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of result to return.</typeparam>
    /// <param name="query">The SQL to execute for the query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static TResult QuerySingle<TResult>(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
    {
        var parts = context.GetConnectionParts();
        return parts.Connection.QuerySingle<TResult>(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of result to return.</typeparam>
    /// <param name="query">The SQL to execute for the query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static TResult? QuerySingleOrDefault<TResult>(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
    {
        var parts = context.GetConnectionParts();
        return parts.Connection.QuerySingleOrDefault<TResult>(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of result to return.</typeparam>
    /// <param name="query">The SQL to execute for the query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static TResult QueryFirst<TResult>(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
    {
        var parts = context.GetConnectionParts();
        return parts.Connection.QueryFirst<TResult>(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of result to return.</typeparam>
    /// <param name="query">The SQL to execute for the query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static TResult? QueryFirstOrDefault<TResult>(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
    {
        var parts = context.GetConnectionParts();
        return parts.Connection.QueryFirstOrDefault<TResult>(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Executes a query, returning the data typed as <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of results to return.</typeparam>
    /// <param name="query">The SQL to execute for the query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="buffered">Whether to buffer results in memory.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static IEnumerable<TResult> Query<TResult>(IQuery query, IConnectionContext context, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) where TResult : Interfaces.QueryBase
    {
        var parts = context.GetConnectionParts();
        return parts.Connection.Query<TResult>(query.QueryText, query.Parameters, parts.Transaction, buffered, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute parameterized SQL that selects a single value.
    /// </summary>
    /// <typeparam name="TResult">The type to return.</typeparam>
    /// <param name="query">The SQL to execute.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The first cell returned, as <typeparamref name="TResult"/>.</returns>
    public static TResult? ExecuteScalar<TResult>(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        return parts.Connection.ExecuteScalar<TResult>(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute parameterized SQL and return an <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="query">The SQL to execute.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>An <see cref="IDataReader"/> that can be used to iterate over the results of the SQL query.</returns>
    /// <remarks>
    /// This is typically used when the results of a query are not processed by Dapper, for example, used to fill a <see cref="DataTable"/>
    /// or <see cref="T:DataSet"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// DataTable table = new DataTable("MyTable");
    /// using (var reader = ExecuteReader(cnn, sql, param))
    /// {
    ///     table.Load(reader);
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public static IDataReader ExecuteReader(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        return parts.Connection.ExecuteReader(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute parameterized SQL.
    /// </summary>
    /// <param name="query">The SQL to execute for this query.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The number of rows affected.</returns>
    public static int Execute(IQuery query, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        return parts.Connection.Execute(query.QueryText, query.Parameters, parts.Transaction, commandTimeout, commandType);
    }
}
