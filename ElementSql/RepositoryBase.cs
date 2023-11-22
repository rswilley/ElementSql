using Dapper;
using Dapper.Contrib.Extensions;
using ElementSql.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace ElementSql
{
    public abstract class RepositoryBase<T> where T : class
    {
        /// <summary>
        /// Returns a single entity by a single id from table "Ts" asynchronously using Task. T must be of interface type. 
        /// Id must be marked with [Key] attribute.
        /// Created entity is tracked/intercepted for changes and used by the Update() extension. 
        /// </summary>
        /// <typeparam name="T">Interface type to create and populate</typeparam>
        /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Entity of T</returns>
        public async Task<T> GetByIdAsync(object id, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.GetAsync<T>(id, parts.Transaction, commandTimeout);
        }

        /// <summary>
        /// Returns a list of entities from table "Ts".  
        /// Id of T must be marked with [Key] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <typeparam name="T">Interface or type to create and populate</typeparam>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Entity of T</returns>
        public async Task<IEnumerable<T>> GetAllAsync(IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.GetAllAsync<T>(parts.Transaction, commandTimeout);
        }

        /// <summary>
        /// Inserts an entity into table "Ts" asynchronously using Task and returns identity id.
        /// </summary>
        /// <typeparam name="T">The type being inserted.</typeparam>
        /// <param name="entity">Entity to insert</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Identity of inserted entity</returns>
        public async Task<T> InsertAsync(T entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            var id = await parts.Connection.InsertAsync(entity, parts.Transaction, commandTimeout);

            TryToSetIdentityProperty(entity, id);

            return entity;
        }

        /// <summary>
        /// Updates entity in table "Ts" asynchronously using Task, checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="entity">Entity to be updated</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public async Task UpdateAsync(T entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            await parts.Connection.UpdateAsync(entity, parts.Transaction, commandTimeout);
        }

        /// <summary>
        /// Delete entity in table "Ts" asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">Entity to delete</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if deleted, false if not found</returns>
        public async Task<bool> DeleteAsync(T entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.DeleteAsync(entity, parts.Transaction, commandTimeout);
        }

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public async Task<T> QuerySingleAsync(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return await parts.Connection.QuerySingleAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static async Task<T?> QuerySingleOrDefaultAsync(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return await parts.Connection.QuerySingleOrDefaultAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static async Task<T> QueryFirstAsync(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return await parts.Connection.QueryFirstAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static async Task<T?> QueryFirstOrDefaultAsync(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return await parts.Connection.QueryFirstOrDefaultAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Execute a query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of results to return.</typeparam>
        /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static async Task<IEnumerable<T>> QueryAsync(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return await parts.Connection.QueryAsync<T>(query, param, parts.Transaction, commandTimeout, commandType);
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
        public static async Task<IDataReader> ExecuteReaderAsync(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return await parts.Connection.ExecuteReaderAsync(query, param, parts.Transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Returns a single entity by a single id from table "Ts".  
        /// Id must be marked with [Key] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <typeparam name="T">Interface or type to create and populate</typeparam>
        /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Entity of T</returns>
        public T GetById(object id, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.Get<T>(id, parts.Transaction, commandTimeout);
        }

        /// <summary>
        /// Returns a list of entities from table "Ts".
        /// Id of T must be marked with [Key] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance.
        /// </summary>
        /// <typeparam name="T">Interface or type to create and populate</typeparam>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Entity of T</returns>
        public IEnumerable<T> GetAll(IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.GetAll<T>(parts.Transaction, commandTimeout);
        }

        /// <summary>
        /// Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <typeparam name="T">The type to insert.</typeparam>
        /// <param name="entity">Entity to insert, can be list of entities</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Identity of inserted entity, or number of inserted rows if inserting a list</returns>
        public T Insert(T entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            var id = parts.Connection.Insert(entity, parts.Transaction, commandTimeout);

            TryToSetIdentityProperty(entity, id);

            return entity;
        }

        /// <summary>
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="entity">Entity to be updated</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public void Update(T entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            parts.Connection.Update(entity, parts.Transaction, commandTimeout);
        }

        /// <summary>
        /// Delete entity in table "Ts".
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">Entity to delete</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if deleted, false if not found</returns>
        public bool Delete(T entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.Delete(entity, parts.Transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a single-row query, returning the data typed as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public T QuerySingle(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return parts.Connection.QuerySingle<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Executes a single-row query, returning the data typed as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T? QuerySingleOrDefault(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return parts.Connection.QuerySingleOrDefault<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Executes a single-row query, returning the data typed as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QueryFirst(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return parts.Connection.QueryFirst<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Executes a single-row query, returning the data typed as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="filter">The SQL filter for the query (WHERE clause or anything after).</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="context">The connection context from Storage Manager</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        /// A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T? QueryFirstOrDefault(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return parts.Connection.QueryFirstOrDefault<T>(query, param, parts.Transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Executes a query, returning the data typed as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of results to return.</typeparam>
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
        public static IEnumerable<T> Query(string filter, object param, IConnectionContext context, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return parts.Connection.Query<T>(query, param, parts.Transaction, buffered, commandTimeout, commandType);
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
        public static IDataReader ExecuteReader(string filter, object param, IConnectionContext context, int? commandTimeout = null, CommandType? commandType = null)
        {
            var parts = context.GetConnectionParts();
            var query = BuildQuery(filter);
            return parts.Connection.ExecuteReader(query, param, parts.Transaction, commandTimeout, commandType);
        }

        private static readonly ConcurrentDictionary<Type, string> _tableNames = new();
        private static readonly ConcurrentDictionary<Type, string> _identities = new();

        private static string BuildQuery(string filter)
        {
            var query = $@"
                SELECT {RepositoryBase<T>.GetColumns()}
                FROM {RepositoryBase<T>.GetTable()}
                {filter}";
            return query;
        }

        private static string GetColumns()
        {
            return ColumnBuilder<T>.GetColumns();
        }

        private static string GetTable()
        {
            var type = typeof(T);

            if (_tableNames.TryGetValue(type, out var tableName))
            {
                return tableName;

            } else
            {
                var attributes = type.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    if (attribute is TableAttribute tableAttribute)
                    {
                        _tableNames.TryAdd(type, tableAttribute.Name);
                        return tableAttribute.Name;
                    }
                }
            }

            throw new Exception("Table attribute is not set on entity.");
        }

        private static void TryToSetIdentityProperty(T entity, dynamic id)
        {
            var type = entity!.GetType();

            PropertyInfo? idProperty;
            if (_identities.TryGetValue(type, out var identityName))
            {
                idProperty = type.GetProperty(identityName);
                idProperty?.SetValue(entity, id);
            }

            idProperty = type.GetProperty("Id");
            if (idProperty?.PropertyType == typeof(int) || idProperty?.PropertyType == typeof(long))
            {
                _identities.TryAdd(type, idProperty.Name);
                idProperty.SetValue(entity, id);
            }
            else
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    var attributes = property.GetCustomAttributes(true);
                    foreach (var attribute in attributes)
                    {
                        if (attribute is KeyAttribute)
                        {
                            _identities.TryAdd(type, property.Name);
                            property.SetValue(entity, id);
                        }
                    }
                }
            }
        }
    }
}
