using System.Linq.Expressions;
namespace ElementSql;

internal sealed class WhereExpressionSqlBuilder
{
    private int _index;
    private readonly Dictionary<string, object?> _parameters = new();

    public (string Sql, Dictionary<string, object?> Parameters) Build<T>(Expression<Func<T, bool>> predicate)
    {
        var sql = Visit(predicate.Body);
        return (sql, _parameters);
    }

    private string Visit(Expression expression)
    {
        return expression switch
        {
            BinaryExpression b => VisitBinary(b),
            MemberExpression m when IsParameterMember(m) => m.Member.Name,
            ConstantExpression c => AddParameter(c.Value),
            UnaryExpression { NodeType: ExpressionType.Convert } u => Visit(u.Operand),
            MemberExpression m => AddParameter(GetValue(m)),
            _ => throw new NotSupportedException($"Unsupported expression: {expression.NodeType}")
        };
    }

    private string VisitBinary(BinaryExpression b)
    {
        var op = b.NodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotSupportedException($"Unsupported binary operator: {b.NodeType}")
        };

        var left = Visit(b.Left);
        var right = Visit(b.Right);
        return $"({left} {op} {right})";
    }

    private string AddParameter(object? value)
    {
        var name = $"p{_index++}";
        _parameters[name] = value;
        return $"@{name}";
    }

    private static bool IsParameterMember(MemberExpression m)
        => m.Expression is ParameterExpression;

    private static object? GetValue(MemberExpression m)
    {
        var objectMember = Expression.Convert(m, typeof(object));
        var getter = Expression.Lambda<Func<object?>>(objectMember).Compile();
        return getter();
    }
}
