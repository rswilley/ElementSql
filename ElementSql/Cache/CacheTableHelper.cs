using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using ElementSql.Attributes;

namespace ElementSql.Cache
{
    internal class CacheTableHelper<T>
    {
        public static string? GetTableKeyColumn()
        {
            var type = typeof(T);

            if (_cachedTables.TryGetValue(type, out var cachedTable))
            {
                if (cachedTable.KeyColumn != null)
                {
                    return cachedTable.KeyColumn;
                }
            }

            return SetCachedTable().KeyColumn;
        }

        public static string GetColumns()
        {
            var type = typeof(T);

            if (_cachedTables.TryGetValue(type, out var cachedTable))
            {
                if (!string.IsNullOrEmpty(cachedTable.ConcatenatedColumns))
                {
                    return cachedTable.ConcatenatedColumns;
                }
            }

            return SetCachedTable().ConcatenatedColumns;
        }

        public static string GetTableName()
        {
            var type = typeof(T);

            if (_cachedTables.TryGetValue(type, out var cachedTable))
            {
                if (!string.IsNullOrEmpty(cachedTable.TableName))
                    return cachedTable.TableName;
            }

            return SetCachedTable().TableName;
        }

        public static string GetInsertStatement()
        {
            var type = typeof(T);

            if (_cachedTables.TryGetValue(type, out var cachedTable))
            {
                if (!string.IsNullOrEmpty(cachedTable.InsertStatement))
                    return cachedTable.InsertStatement;
            }

            return SetCachedTable().InsertStatement;
        }

        public static string GetUpdateStatement()
        {
            var type = typeof(T);

            if (_cachedTables.TryGetValue(type, out var cachedTable))
            {
                if (!string.IsNullOrEmpty(cachedTable.UpdateStatement))
                    return cachedTable.UpdateStatement;
            }

            return SetCachedTable().UpdateStatement;
        }

        public static void TryToSetIdentityProperty(T entity, dynamic id)
        {
            var type = entity!.GetType();
            var key = string.Empty;

            if (_cachedTables.TryGetValue(type, out var cachedTable))
            {
                if (cachedTable.KeyColumn != null)
                {
                    key = cachedTable.KeyColumn;
                }
            }
            else
            {
                key = SetCachedTable().KeyColumn;
            }

            if (string.IsNullOrEmpty(key))
                return;

            PropertyInfo? idProperty = type.GetProperty(key);
            idProperty?.SetValue(entity, id);
        }

        private static readonly ConcurrentDictionary<Type, CachedTable> _cachedTables = new();

        private static CachedTable SetCachedTable()
        {
            var type = typeof(T);
            var attributes = type.GetCustomAttributes(false);

            var tableName = string.Empty;
            var columns = GetColumnsFromEntity();

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

            var insertStatement = BuildInsertStatement(tableName, columns);
            var updateStatement = BuildUpdateStatement(tableName, columns);

            var cachedTable = new CachedTable
            {
                TableName = tableName,
                Columns = columns,
                KeyColumn = columns.SingleOrDefault(c => c.IsKey)?.Name,
                ConcatenatedColumns = insertStatement.allConcatenatedColumns,
                InsertStatement = insertStatement.insertStatement,
                UpdateStatement = updateStatement
            };
            _cachedTables.TryAdd(type, cachedTable);

            return cachedTable;
        }

        private static (string allConcatenatedColumns, string insertStatement) BuildInsertStatement(string tableName, IEnumerable<CachedColumn> columns)
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

            return (allConcatenatedColumns, $"INSERT INTO {tableName} ({concatenatedColumns}) VALUES ({valuesConcatenated});");
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

            return $"UPDATE {tableName} SET {valuesConcatenated} WHERE {keyColumn}=@Key;";
        }

        private static IEnumerable<CachedColumn> GetColumnsFromEntity()
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
                    if (attribute is KeyAttribute)
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

        private static string GetColumnName(PropertyInfo property)
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
}
