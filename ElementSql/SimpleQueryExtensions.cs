using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using ElementSql.Attributes;
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
        var builder = new WhereExpressionBuilder();
        var where = builder.Build(predicate.Body);

        var queryText = $"""
                         SELECT {CacheTableHelper.GetColumns<TEntity>()}
                         FROM {CacheTableHelper.GetTableName<TEntity>()}
                         WHERE {where}
                         """;

        return new SqlQuery(queryText, builder.Parameters);
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

    private sealed class WhereExpressionBuilder
    {
        private int _parameterIndex;

        public Dictionary<string, object> Parameters { get; } = new();

        public string Build(Expression expression)
        {
            return expression switch
            {
                BinaryExpression binary => BuildBinary(binary),
                UnaryExpression { NodeType: ExpressionType.Convert } unary => Build(unary.Operand),
                MemberExpression member when IsEntityProperty(member) => GetColumnName(member.Member),
                ConstantExpression constant => AddParameter(constant.Value),
                MemberExpression member => AddParameter(GetMemberValue(member)),
                _ => throw new NotSupportedException($"Unsupported expression type '{expression.NodeType}'.")
            };
        }

        private string BuildBinary(BinaryExpression binary)
        {
            var sqlOperator = binary.NodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                _ => throw new NotSupportedException($"Operator '{binary.NodeType}' is not supported.")
            };

            var left = Build(binary.Left);
            var right = Build(binary.Right);
            return $"({left} {sqlOperator} {right})";
        }

        private string AddParameter(object? value)
        {
            var name = $"p{_parameterIndex++}";
            Parameters[name] = value!;
            return $"@{name}";
        }

        private static bool IsEntityProperty(MemberExpression member)
        {
            return member.Expression is ParameterExpression;
        }

        private static string GetColumnName(MemberInfo member)
        {
            return member.GetCustomAttribute<ColumnAttribute>()?.Name ?? member.Name;
        }

        private static object? GetMemberValue(MemberExpression member)
        {
            var converted = Expression.Convert(member, typeof(object));
            var getter = Expression.Lambda<Func<object?>>(converted).Compile();
            return getter();
        }
    }
}

public sealed record SqlQuery(string QueryText, Dictionary<string, object> Parameters) : IQuery;
