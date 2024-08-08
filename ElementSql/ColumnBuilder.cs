using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using ElementSql.Attributes;

namespace ElementSql
{
    internal class ColumnBuilder<T>
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
            } else
            {
                key = SetCachedTable().KeyColumn;
            }

            if (string.IsNullOrEmpty(key))
                return;

            PropertyInfo? idProperty = type.GetProperty(key);
            idProperty?.SetValue(entity, id);
        }

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
                throw new Exception("Table attribute is not set on entity.");
            }
            if (!columns.Any())
            {
                throw new Exception("Entity has no columns.");
            }

            var insertStatement = BuildInsertStatement(tableName, columns);
            var updateStatement = BuildUpdateStatement(tableName, columns);

            var cachedTable = new CachedTable
            {
                TableName = tableName,
                Columns = columns,
                KeyColumn = columns.SingleOrDefault(c => c.IsKey)?.Name,
                ConcatenatedColumns = string.Join(',', columns),
                InsertStatement = insertStatement,
                UpdateStatement = updateStatement
            };
            _cachedTables.TryAdd(type, cachedTable);

            return cachedTable;
        }

        private static string BuildInsertStatement(string tableName, IEnumerable<CachedColumn> columns)
        {
            var columnConcatenated = string.Empty;
            var valuesConcatenated = string.Empty;

            foreach (var column in columns)
            {
                if (column.IsKey)
                {
                    continue;
                }

                columnConcatenated += $"{column.Name},";
                valuesConcatenated += $"@{column.Name},";
            }

            columnConcatenated = columnConcatenated.TrimEnd(',');
            valuesConcatenated = valuesConcatenated.TrimEnd(',');

            return $"INSERT INTO {tableName} ({columnConcatenated}) VALUES ({valuesConcatenated});";
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
                }

                valuesConcatenated += $"{column.Name}=@{column.Name}";
            }

            valuesConcatenated = valuesConcatenated.TrimEnd(',');

            return $"UPDATE {tableName} SET ({valuesConcatenated}) WHERE {keyColumn} = @Key;";
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

        private static readonly ConcurrentDictionary<Type, CachedTable> _cachedTables = new();

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

internal class CachedTable
{
    public string TableName { get; set; } = string.Empty;
    public IEnumerable<CachedColumn> Columns { get; set; } = Enumerable.Empty<CachedColumn>();
    public string? KeyColumn { get; set; }
    public string ConcatenatedColumns { get; set; } = string.Empty;
    public string InsertStatement { get; set; } = string.Empty;
    public string UpdateStatement { get; set; } = string.Empty;
}

internal class CachedColumn
{
    public bool IsKey { get; set; }
    public string Name { get; set; } = string.Empty;
}
