using System.Data;
using System.Linq.Expressions;
using ElementSql.Cache;
using ElementSql.Interfaces;

namespace ElementSql;

public static class SimpleQueryExtensions
{
    public static Task<TResult> QueryFirstAsync<TResult>(
        this IConnectionContext context,
        IQuery<TResult> query,
        int? commandTimeout = null,
        CommandType? commandType = null)
        where TResult : Interfaces.QueryBase
        => context.QueryFirstAsync<TResult>(query, commandTimeout, commandType);

    public static Task<TResult?> QueryFirstOrDefaultAsync<TResult>(
        this IConnectionContext context,
        IQuery<TResult> query,
        int? commandTimeout = null,
        CommandType? commandType = null)
        where TResult : Interfaces.QueryBase
        => context.QueryFirstOrDefaultAsync<TResult>(query, commandTimeout, commandType);

    public static Task<TResult> QuerySingleAsync<TResult>(
        this IConnectionContext context,
        IQuery<TResult> query,
        int? commandTimeout = null,
        CommandType? commandType = null)
        where TResult : Interfaces.QueryBase
        => context.QuerySingleAsync<TResult>(query, commandTimeout, commandType);

    public static Task<TResult?> QuerySingleOrDefaultAsync<TResult>(
        this IConnectionContext context,
        IQuery<TResult> query,
        int? commandTimeout = null,
        CommandType? commandType = null)
        where TResult : Interfaces.QueryBase
        => context.QuerySingleOrDefaultAsync<TResult>(query, commandTimeout, commandType);

    public static Task<IEnumerable<TResult>> QueryAsync<TResult>(
        this IConnectionContext context,
        IQuery<TResult> query,
        int? commandTimeout = null,
        CommandType? commandType = null)
        where TResult : Interfaces.QueryBase
        => context.QueryAsync<TResult>(query, commandTimeout, commandType);

    public static IQuery Where<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Interfaces.QueryBase
    {
        var builder = new WhereExpressionSqlBuilder();
        var (where, parameters) = builder.Build(predicate);

        var queryText = $"""
                         SELECT {CacheTableHelper.GetColumns<TEntity>()}
                         FROM {CacheTableHelper.GetTableName<TEntity>()}
                         WHERE {where}
                         """;

        return new SqlQuery(queryText, parameters);
    }

    public static Task<IEnumerable<TEntity>> WhereAsync<TEntity>(
        this IConnectionContext context,
        Expression<Func<TEntity, bool>> predicate,
        int? commandTimeout = null,
        CommandType? commandType = null)
        where TEntity : Interfaces.QueryBase
    {
        // Ensure cache metadata is initialized for the active connection before building SQL.
        context.GetConnectionParts();
        return context.QueryAsync<TEntity>(Where(predicate), commandTimeout, commandType);
    }

    public static Task<TEntity?> FirstOrDefaultWhereAsync<TEntity>(
        this IConnectionContext context,
        Expression<Func<TEntity, bool>> predicate,
        int? commandTimeout = null,
        CommandType? commandType = null)
        where TEntity : Interfaces.QueryBase
    {
        context.GetConnectionParts();
        return context.QueryFirstOrDefaultAsync<TEntity>(Where(predicate), commandTimeout, commandType);
    }

}

public sealed record SqlQuery(string QueryText, Dictionary<string, object> Parameters) : IQuery;
