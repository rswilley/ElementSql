using System.Data;
using Dapper;
using ElementSql.Cache;
using ElementSql.Interfaces;

namespace ElementSql;

internal static class EntityHelper
{
    /// <summary>
    /// Returns a single entity by a single id from table "Ts" asynchronously using Task. T must be of interface type.
    /// Id must be marked with [Key] attribute.
    /// Created entity is tracked/intercepted for changes and used by the Update() extension.
    /// </summary>
    /// <typeparam name="TEntity">Interface type to create and populate</typeparam>
    /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <returns>Entity of T</returns>
    internal static async Task<TEntity?> GetByIdAsync<TEntity>(object id, IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>("WHERE Id = @Id");

        return await parts.Connection.QuerySingleOrDefaultAsync<TEntity>(query, new { id }, parts.Transaction, commandTimeout);
    }

    /// <summary>
    /// Returns a list of entities from table "Ts".
    /// Id of T must be marked with [Key] attribute.
    /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
    /// for optimal performance.
    /// </summary>
    /// <typeparam name="TEntity">Interface or type to create and populate</typeparam>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <returns>Entity of T</returns>
    internal static async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>("");

        return await parts.Connection.QueryAsync<TEntity>(query, new { }, parts.Transaction, commandTimeout);
    }

    /// <summary>
    /// Inserts an entity into table "Ts" asynchronously using Task and returns identity id.
    /// </summary>
    /// <typeparam name="TEntity">The type being inserted.</typeparam>
    /// <param name="entity">Entity to insert</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <returns>Identity of inserted entity</returns>
    internal static async Task<TEntity> InsertAsync<TEntity>(TEntity entity, IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        var insertCommand = CacheTableHelper.GetInsertStatement<TEntity>();
        var id = await parts.Connection.ExecuteScalarAsync(insertCommand, entity, parts.Transaction, commandTimeout);

        CacheTableHelper.TryToSetIdentityProperty(entity, id!);

        return entity;
    }

    /// <summary>
    /// Updates entity in table "Ts" asynchronously using Task
    /// </summary>
    /// <typeparam name="TEntity">Type to be updated</typeparam>
    /// <param name="entity">Entity to be updated</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    internal static async Task UpdateAsync<TEntity>(TEntity entity, IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        var updateCommand = CacheTableHelper.GetUpdateStatement<TEntity>();
        await parts.Connection.ExecuteAsync(updateCommand, entity, parts.Transaction, commandTimeout);
    }

    /// <summary>
    /// Either inserts or updates entity in table "Ts" asynchronously using Task
    /// </summary>
    /// <typeparam name="TEntity">Type to be upserted</typeparam>
    /// <param name="entity">Entity to be upserted</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    internal static async Task UpsertAsync<TEntity>(TEntity entity, IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var shouldInsert = false;
        switch (entity.Id)
        {
            case long or int:
            {
                var id = Convert.ToInt64(entity.Id);
                if (id == 0)
                    shouldInsert = true;
                break;
            }
            case Guid id:
            {
                if (id == Guid.Empty)
                    shouldInsert = true;
                break;
            }
            default:
                throw new Exception("Column Id must be an int, long or Guid type");
        }

        if (shouldInsert)
        {
            await InsertAsync(entity, context, commandTimeout);
        }
        else
        {
            await UpdateAsync(entity, context, commandTimeout);
        }
    }

    /// <summary>
    /// Delete entity in table "Ts" asynchronously using Task.
    /// </summary>
    /// <param name="entity">Entity to be deleted</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    internal static async Task DeleteAsync<TEntity>(TEntity entity, IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        var command = $"DELETE FROM {CacheTableHelper.GetTableName<TEntity>()} WHERE {CacheTableHelper.GetTableKeyColumn<TEntity>()} = @Key";

        await parts.Connection.ExecuteAsync(command, new { Key = entity.Id }, parts.Transaction, commandTimeout);
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="TEntity">The type of result to return.</typeparam>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    internal static async Task<TEntity> QuerySingleAsync<TEntity>(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return await parts.Connection.QuerySingleAsync<TEntity>(query, param, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="TEntity">The type to return.</typeparam>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    internal static async Task<TEntity?> QuerySingleOrDefaultAsync<TEntity>(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return await parts.Connection.QuerySingleOrDefaultAsync<TEntity>(query, param, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="TEntity">The type of result to return.</typeparam>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    internal static async Task<TEntity> QueryFirstAsync<TEntity>(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return await parts.Connection.QueryFirstAsync<TEntity>(query, param, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="TEntity">The type of result to return.</typeparam>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    internal static async Task<TEntity?> QueryFirstOrDefaultAsync<TEntity>(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return await parts.Connection.QueryFirstOrDefaultAsync<TEntity>(query, param, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute a query asynchronously using Task.
    /// </summary>
    /// <typeparam name="TEntity">The type of results to return.</typeparam>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="TEntity"/>; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    internal static async Task<IEnumerable<TEntity>> QueryAsync<TEntity>(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return await parts.Connection.QueryAsync<TEntity>(query, param, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute parameterized SQL and return an <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
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
    internal static async Task<IDataReader> ExecuteReaderAsync<TEntity>(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return await parts.Connection.ExecuteReaderAsync(query, param, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Returns a single entity by a single id from table "Ts".
    /// Id must be marked with [Key] attribute.
    /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
    /// for optimal performance.
    /// </summary>
    /// <typeparam name="TEntity">Interface or type to create and populate</typeparam>
    /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <returns>Entity of T</returns>
    internal static TEntity? GetById<TEntity>(object id, IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>("WHERE Id = @Id");

        return parts.Connection.QuerySingleOrDefault<TEntity>(query, new { id }, parts.Transaction, commandTimeout);
    }

    /// <summary>
    /// Returns a list of entities from table "Ts".
    /// Id of T must be marked with [Key] attribute.
    /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
    /// for optimal performance.
    /// </summary>
    /// <typeparam name="TEntity">Interface or type to create and populate</typeparam>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <returns>Entity of T</returns>
    internal static IEnumerable<TEntity> GetAll<TEntity>(IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>("");

        return parts.Connection.Query<TEntity>(query, null, parts.Transaction, true, commandTimeout);
    }

    /// <summary>
    /// Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
    /// </summary>
    /// <typeparam name="TEntity">The type to insert.</typeparam>
    /// <param name="entity">Entity to insert, can be list of entities</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <returns>Identity of inserted entity, or number of inserted rows if inserting a list</returns>
    internal static TEntity Insert<TEntity>(TEntity entity, IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        var insertCommand = CacheTableHelper.GetInsertStatement<TEntity>();
        var id = parts.Connection.ExecuteScalar(insertCommand, entity, parts.Transaction, commandTimeout);

        CacheTableHelper.TryToSetIdentityProperty(entity, id!);

        return entity;
    }

    /// <summary>
    /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
    /// </summary>
    /// <typeparam name="TEntity">Type to be updated</typeparam>
    /// <param name="entity">Entity to be updated</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
    internal static void Update<TEntity>(TEntity entity, IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        parts.Connection.Execute(CacheTableHelper.GetUpdateStatement<TEntity>(), entity, parts.Transaction, commandTimeout);
    }

    /// <summary>
    /// Delete entity in table "Ts".
    /// </summary>
    /// <param name="key">key to delete</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    internal static void Delete<TEntity>(uint key, IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        var command = $"DELETE FROM {CacheTableHelper.GetTableName<TEntity>()} WHERE {CacheTableHelper.GetTableKeyColumn<TEntity>()} = @Key";

        parts.Connection.Execute(command, new { key }, parts.Transaction, commandTimeout);
    }

    /// <summary>
    /// Delete entity in table "Ts".
    /// </summary>
    /// <param name="key">key to delete</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    internal static void Delete<TEntity>(ulong key, IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        var command = $"DELETE FROM {CacheTableHelper.GetTableName<TEntity>()} WHERE {CacheTableHelper.GetTableKeyColumn<TEntity>()} = @Key";

        parts.Connection.Execute(command, new { key }, parts.Transaction, commandTimeout);
    }

    /// <summary>
    /// Delete entity in table "Ts".
    /// </summary>
    /// <param name="key">key to delete</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
    internal static void Delete<TEntity>(Guid key, IConnectionContext context, int? commandTimeout = null) where TEntity : IEntityBase
    {
        var parts = context.GetConnectionParts();
        var command = $"DELETE FROM {CacheTableHelper.GetTableName<TEntity>()} WHERE {CacheTableHelper.GetTableKeyColumn<TEntity>()} = @Key";

        parts.Connection.Execute(command, new { key }, parts.Transaction, commandTimeout);
    }

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of result to return.</typeparam>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    internal static TEntity QuerySingle<TEntity>(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return parts.Connection.QuerySingle<TEntity>(query, param, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of result to return.</typeparam>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    internal static TEntity? QuerySingleOrDefault<TEntity>(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return parts.Connection.QuerySingleOrDefault<TEntity>(query, param, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of result to return.</typeparam>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    internal static TEntity QueryFirst<TEntity>(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return parts.Connection.QueryFirst<TEntity>(query, param, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Executes a single-row query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of result to return.</typeparam>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    internal static TEntity? QueryFirstOrDefault<TEntity>(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return parts.Connection.QueryFirstOrDefault<TEntity>(query, param, parts.Transaction, commandTimeout, commandType);
    }

    /// <summary>
    /// Executes a query, returning the data typed as <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of results to return.</typeparam>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="buffered">Whether to buffer results in memory.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    internal static IEnumerable<TEntity> Query<TEntity>(string filter, object param, IConnectionContext context, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return parts.Connection.Query<TEntity>(query, param, parts.Transaction, buffered, commandTimeout, commandType);
    }

    /// <summary>
    /// Execute parameterized SQL and return an <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="context">The connection context from Storage Manager</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
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
    internal static IDataReader ExecuteReader<TEntity>(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
    {
        var parts = context.GetConnectionParts();
        var query = BuildQuery<TEntity>(filter);
        return parts.Connection.ExecuteReader(query, param, parts.Transaction, commandTimeout, commandType);
    }

    private static string BuildQuery<TEntity>(string filter)
    {
        var query = $"""
                     SELECT {CacheTableHelper.GetColumns<TEntity>()}
                     FROM {CacheTableHelper.GetTableName<TEntity>()}
                     {filter}
                     """;
        return query;
    }
}
