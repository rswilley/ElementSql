namespace ElementSql.Interfaces;

//marker interface
public interface IElementSqlQuery;

public interface IQuery
{
    string QueryText { get; }
    Dictionary<string, object> Parameters { get; }
}

public interface IQuery<out TResult> : IQuery;
