using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using ElementSql.Attributes;
namespace ElementSql;

internal sealed class WhereExpressionSqlBuilder
{
    private int _index;
    private readonly Dictionary<string, object> _parameters = new();

    public (string Sql, Dictionary<string, object> Parameters) Build<T>(Expression<Func<T, bool>> predicate)
    {
        var sql = Visit(predicate.Body);
        return (sql, _parameters);
    }

    private string Visit(Expression expression)
    {
        return expression switch
        {
            BinaryExpression b => VisitBinary(b),
            MethodCallExpression m => VisitMethodCall(m),
            UnaryExpression { NodeType: ExpressionType.Not } u => $"(NOT {Visit(u.Operand)})",
            MemberExpression m when IsParameterMember(m) => GetColumnName(m.Member),
            ConstantExpression c => AddParameter(c.Value),
            UnaryExpression { NodeType: ExpressionType.Convert } u => Visit(u.Operand),
            MemberExpression m => AddParameter(GetValue(m)),
            _ => throw new NotSupportedException($"Unsupported expression: {expression.NodeType}")
        };
    }

    private string VisitBinary(BinaryExpression b)
    {
        if (b.NodeType is ExpressionType.AndAlso or ExpressionType.OrElse)
        {
            var logicalOperator = b.NodeType == ExpressionType.AndAlso ? "AND" : "OR";
            var leftLogical = Visit(b.Left);
            var rightLogical = Visit(b.Right);
            return $"({leftLogical} {logicalOperator} {rightLogical})";
        }

        if (TryBuildNullComparison(b, out var nullComparison))
        {
            return nullComparison;
        }

        var op = b.NodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            _ => throw new NotSupportedException($"Unsupported binary operator: {b.NodeType}")
        };

        var left = Visit(b.Left);
        var right = Visit(b.Right);
        return $"({left} {op} {right})";
    }

    private string VisitMethodCall(MethodCallExpression expression)
    {
        if (TryVisitStringMethod(expression, out var stringSql))
        {
            return stringSql;
        }

        if (TryVisitInClause(expression, out var inSql))
        {
            return inSql;
        }

        throw new NotSupportedException($"Unsupported method call: {expression.Method.Name}");
    }

    private bool TryVisitStringMethod(MethodCallExpression expression, out string sql)
    {
        sql = string.Empty;
        if (expression.Object is not MemberExpression member ||
            !IsParameterMember(member) ||
            expression.Arguments.Count != 1 ||
            expression.Method.DeclaringType != typeof(string))
        {
            return false;
        }

        var column = GetColumnName(member.Member);
        var value = GetValue(expression.Arguments[0]);

        sql = expression.Method.Name switch
        {
            nameof(string.Contains) => $"({column} LIKE {AddParameter($"%{value}%")})",
            nameof(string.StartsWith) => $"({column} LIKE {AddParameter($"{value}%")})",
            nameof(string.EndsWith) => $"({column} LIKE {AddParameter($"%{value}")})",
            _ => string.Empty
        };

        return !string.IsNullOrWhiteSpace(sql);
    }

    private bool TryVisitInClause(MethodCallExpression expression, out string sql)
    {
        sql = string.Empty;
        if (!string.Equals(expression.Method.Name, nameof(Enumerable.Contains), StringComparison.Ordinal) || expression.Arguments.Count == 0)
        {
            return false;
        }

        if (!TryGetContainsParts(expression, out var collection, out var column))
        {
            return false;
        }

        var materialized = MaterializeEnumerable(collection);
        if (materialized.Count == 0)
        {
            sql = "(1 = 0)";
            return true;
        }

        sql = $"({column} IN {AddParameter(materialized)})";
        return true;
    }

    private static bool TryGetContainsParts(MethodCallExpression expression, out IEnumerable collection, out string column)
    {
        collection = Array.Empty<object>();
        column = string.Empty;

        if (expression.Object != null)
        {
            if (expression.Object.Type == typeof(string) || expression.Arguments.Count != 1)
            {
                return false;
            }

            var objectValue = GetValue(expression.Object);
            if (objectValue is not IEnumerable objectEnumerable || objectValue is string)
            {
                return false;
            }

            if (!TryGetColumnName(expression.Arguments[0], out column))
            {
                return false;
            }

            collection = objectEnumerable;
            return true;
        }

        if (expression.Method.DeclaringType != typeof(Enumerable) || expression.Arguments.Count != 2)
        {
            return false;
        }

        var sourceValue = GetValue(expression.Arguments[0]);
        if (sourceValue is not IEnumerable sourceEnumerable || sourceValue is string)
        {
            return false;
        }

        if (!TryGetColumnName(expression.Arguments[1], out column))
        {
            return false;
        }

        collection = sourceEnumerable;
        return true;
    }

    private static List<object?> MaterializeEnumerable(IEnumerable values)
    {
        var materialized = new List<object?>();
        foreach (var value in values)
        {
            materialized.Add(value);
        }

        return materialized;
    }

    private string AddParameter(object? value)
    {
        var name = $"p{_index++}";
        _parameters[name] = value ?? DBNull.Value;
        return $"@{name}";
    }

    private bool TryBuildNullComparison(BinaryExpression expression, out string sql)
    {
        sql = string.Empty;
        if (expression.NodeType is not (ExpressionType.Equal or ExpressionType.NotEqual))
        {
            return false;
        }

        if (TryGetColumnWhenComparedWithNull(expression.Left, expression.Right, out var column) ||
            TryGetColumnWhenComparedWithNull(expression.Right, expression.Left, out column))
        {
            var comparison = expression.NodeType == ExpressionType.Equal ? "IS NULL" : "IS NOT NULL";
            sql = $"({column} {comparison})";
            return true;
        }

        return false;
    }

    private static bool TryGetColumnWhenComparedWithNull(Expression potentialColumn, Expression potentialNull, out string column)
    {
        column = string.Empty;
        if (!TryGetColumnName(potentialColumn, out column))
        {
            return false;
        }

        return IsNullValue(potentialNull);
    }

    private static bool TryGetColumnName(Expression expression, out string column)
    {
        column = string.Empty;
        if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            return TryGetColumnName(unary.Operand, out column);
        }

        if (expression is MemberExpression member && IsParameterMember(member))
        {
            column = GetColumnName(member.Member);
            return true;
        }

        return false;
    }

    private static bool IsNullValue(Expression expression)
    {
        if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            return IsNullValue(unary.Operand);
        }

        if (expression is ConstantExpression constant)
        {
            return constant.Value is null;
        }

        if (expression is MemberExpression member && !IsParameterMember(member))
        {
            return GetValue(member) is null;
        }

        return false;
    }

    private static bool IsParameterMember(MemberExpression m)
        => m.Expression is ParameterExpression;

    private static object? GetValue(MemberExpression m)
    {
        var objectMember = Expression.Convert(m, typeof(object));
        var getter = Expression.Lambda<Func<object?>>(objectMember).Compile();
        return getter();
    }

    private static object? GetValue(Expression expression)
    {
        if (expression is ConstantExpression constant)
        {
            return constant.Value;
        }

        var converted = Expression.Convert(expression, typeof(object));
        var getter = Expression.Lambda<Func<object?>>(converted).Compile();
        return getter();
    }

    private static string GetColumnName(MemberInfo member)
    {
        return member.GetCustomAttribute<ColumnAttribute>()?.Name ?? member.Name;
    }
}
