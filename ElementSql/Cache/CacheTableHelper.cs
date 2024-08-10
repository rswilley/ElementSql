using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using ElementSql.Attributes;

namespace ElementSql.Cache
{
    internal static class CacheTableHelper
    {
        private static SqlFlavor? _sqlFlavor;
        
        public static void Initialize(IDbConnection connection)
        {
            var name = connection.GetType().Name.ToLower();
            _sqlFlavor = SqlFlavorStrategy[name];
        }
        
        public static string? GetTableKeyColumn<T>()
        {
            if (_sqlFlavor is null)
            {
                throw new Exception("CacheTableHelper is not initialized");
            }
            
            var type = typeof(T);

            if (CachedTables.TryGetValue(type, out var cachedTable))
            {
                if (cachedTable.KeyColumnName != null)
                {
                    return cachedTable.KeyColumnName;
                }
            }

            return SetCachedTable<T>().KeyColumnName;
        }

        public static string GetColumns<T>()
        {
            if (_sqlFlavor is null)
            {
                throw new Exception("CacheTableHelper is not initialized");
            }
            
            var type = typeof(T);

            if (CachedTables.TryGetValue(type, out var cachedTable))
            {
                if (!string.IsNullOrEmpty(cachedTable.ConcatenatedColumns))
                {
                    return cachedTable.ConcatenatedColumns;
                }
            }

            return SetCachedTable<T>().ConcatenatedColumns;
        }

        public static string GetTableName<T>()
        {
            if (_sqlFlavor is null)
            {
                throw new Exception("CacheTableHelper is not initialized");
            }
            
            var type = typeof(T);

            if (CachedTables.TryGetValue(type, out var cachedTable))
            {
                if (!string.IsNullOrEmpty(cachedTable.TableName))
                    return cachedTable.TableName;
            }

            return SetCachedTable<T>().TableName;
        }

        public static string GetInsertStatement<T>()
        {
            if (_sqlFlavor is null)
            {
                throw new Exception("CacheTableHelper is not initialized");
            }
            
            var type = typeof(T);

            if (CachedTables.TryGetValue(type, out var cachedTable))
            {
                if (!string.IsNullOrEmpty(cachedTable.InsertStatement))
                    return cachedTable.InsertStatement;
            }

            return SetCachedTable<T>().InsertStatement;
        }

        public static string GetUpdateStatement<T>()
        {
            if (_sqlFlavor is null)
            {
                throw new Exception("CacheTableHelper is not initialized");
            }
            
            var type = typeof(T);

            if (CachedTables.TryGetValue(type, out var cachedTable))
            {
                if (!string.IsNullOrEmpty(cachedTable.UpdateStatement))
                    return cachedTable.UpdateStatement;
            }

            return SetCachedTable<T>().UpdateStatement;
        }

        public static void TryToSetIdentityProperty<T>(T entity, dynamic id)
        {
            if (_sqlFlavor is null)
            {
                throw new Exception("CacheTableHelper is not initialized");
            }
            
            var type = entity!.GetType();
            var key = string.Empty;

            if (CachedTables.TryGetValue(type, out var cachedTable))
            {
                if (cachedTable.KeyColumnName != null)
                {
                    key = cachedTable.KeyColumnName;
                }
            }
            else
            {
                key = SetCachedTable<T>().KeyColumnName;
            }

            if (string.IsNullOrEmpty(key))
                return;

            var idProperty = type.GetProperty(key);
            idProperty?.SetValue(entity, id);
        }

        private static readonly ConcurrentDictionary<Type, CachedTable> CachedTables = new();
        private static Dictionary<string, SqlFlavor> SqlFlavorStrategy = new()
        {
            {"sqlconnection", new SqlFlavor
            {
                InsertSelectId = "; SELECT SCOPE_IDENTITY() %key%",
                ColumnPrefix = "[",
                ColumnSuffix = "]"
            }},
            {"sqlceconnection", new SqlFlavor()}, //TODO
            {"npgsqlconnection", new SqlFlavor
            {
                InsertSelectId = " RETURNING %key%",
                ColumnPrefix = "",
                ColumnSuffix = ""
            }},
            {"sqliteconnection", new SqlFlavor()}, //TODO
            {"mysqlconnection", new SqlFlavor
            {
                InsertSelectId = "; SELECT LAST_INSERT_ID() %key%",
                ColumnPrefix = "`",
                ColumnSuffix = "`"
            }},
            {"fbconnection", new SqlFlavor()}, //TODO
        };

        private static CachedTable SetCachedTable<T>()
        {
            var type = typeof(T);
            var attributes = type.GetCustomAttributes(false);

            var tableName = string.Empty;
            var columns = GetColumnsFromEntity<T>().ToList();

            foreach (var attribute in attributes)
            {
                if (attribute is TableAttribute tableAttribute)
                {
                    tableName = tableAttribute.Name;
                }
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new Exception($"Table attribute is not set on entity: {type.Name}");
            }
            if (!columns.Any())
            {
                throw new Exception($"No columns found on entity: {type.Name}");
            }

            var keyColumns = columns.Where(c => c.IsKey).ToList();
            if (!keyColumns.Any())
            {
                throw new Exception($"Must set at least one Key or ExplicitKey on entity: {type.Name}");
            }

            if (keyColumns.Count() > 1)
            {
                throw new Exception($"Only one Key or ExplicitKey allowed on entity: {type.Name}");
            }

            var keyColumnName = columns.Single(c => c.IsKey).Name;
            var insertStatement = BuildInsertStatement(tableName, keyColumnName, columns);
            var updateStatement = BuildUpdateStatement(tableName, columns);

            var cachedTable = new CachedTable
            {
                TableName = tableName,
                Columns = columns,
                KeyColumnName = keyColumnName,
                ConcatenatedColumns = insertStatement.allConcatenatedColumns,
                InsertStatement = insertStatement.insertStatement,
                UpdateStatement = updateStatement
            };
            CachedTables.TryAdd(type, cachedTable);

            return cachedTable;
        }

        private static (string allConcatenatedColumns, string insertStatement) BuildInsertStatement(string tableName, string keyColumn, IEnumerable<CachedColumn> columns)
        {
            var allConcatenatedColumns = string.Empty;
            var concatenatedColumns = string.Empty;
            var valuesConcatenated = string.Empty;

            foreach (var column in columns)
            {
                allConcatenatedColumns += $"{column.Name},";

                if (column.IsKey)
                {
                    continue;
                }

                concatenatedColumns += $"{column.Name},";
                valuesConcatenated += $"@{column.Name},";
            }

            allConcatenatedColumns = allConcatenatedColumns.TrimEnd(',');
            concatenatedColumns = concatenatedColumns.TrimEnd(',');
            valuesConcatenated = valuesConcatenated.TrimEnd(',');

            var insert = $"INSERT INTO {tableName} ({concatenatedColumns}) VALUES ({valuesConcatenated})";
            var insertId = _sqlFlavor.InsertSelectId;
            var insertCommand = insert + insertId.Replace("%key%", keyColumn);

            return (allConcatenatedColumns, insertCommand);
        }

        private static string BuildUpdateStatement(string tableName, IEnumerable<CachedColumn> columns)
        {
            var valuesConcatenated = string.Empty;
            var keyColumn = string.Empty;

            foreach (var column in columns)
            {
                if (column.IsKey)
                {
                    keyColumn = column.Name;
                } else
                {
                    valuesConcatenated += $"{column.Name}=@{column.Name},";
                }
            }

            valuesConcatenated = valuesConcatenated.TrimEnd(',');

            return $"UPDATE {tableName} SET {valuesConcatenated} WHERE {keyColumn}=@Id;";
        }

        private static IEnumerable<CachedColumn> GetColumnsFromEntity<T>()
        {
            var columns = new List<CachedColumn>();

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(true);
                var isKey = false;

                foreach (var attribute in attributes)
                {
                    if (attribute is KeyAttribute or ExplicitKeyAttribute)
                    {
                        isKey = true;
                        break;
                    }
                }

                columns.Add(new CachedColumn
                {
                    IsKey = isKey,
                    Name = GetColumnName(property)
                });
            }

            return columns;
        }

        private static string GetColumnName(MemberInfo property)
        {
            var attrs = property.GetCustomAttributes();
            foreach (var attr in attrs)
            {
                if (attr is ColumnAttribute c)
                {
                    return c.Name;
                }
            }

            return property.Name;
        }
    }

    internal class SqlFlavor
    {
        public string InsertSelectId { get; set; } = string.Empty;
        public string ColumnPrefix { get; set; } = string.Empty;
        public string ColumnSuffix { get; set; } = string.Empty;
    }
}
