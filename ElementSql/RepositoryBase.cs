using Dapper.Contrib.Extensions;
using ElementSql.Interfaces;

namespace ElementSql
{
    public abstract class RepositoryBase<TEntity> : QueryBase where TEntity : class
    {
        public async Task<TEntity> GetByIdAsync(object id, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.GetAsync<TEntity>(id, parts.Transaction, commandTimeout);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.GetAllAsync<TEntity>(parts.Transaction, commandTimeout);
        }

        public async Task<TEntity> AddAsync(TEntity entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            var id = await parts.Connection.InsertAsync(entity, parts.Transaction, commandTimeout);

            TryToSetIdentityProperty(entity, id);

            return entity;
        }

        public async Task UpdateAsync(TEntity entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            await parts.Connection.UpdateAsync(entity, parts.Transaction, commandTimeout);
        }

        public async Task<bool> DeleteAsync(TEntity entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.DeleteAsync(entity, parts.Transaction, commandTimeout);
        }

        public TEntity GetById(object id, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.Get<TEntity>(id, parts.Transaction, commandTimeout);
        }

        public IEnumerable<TEntity> GetAll(IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.GetAll<TEntity>(parts.Transaction, commandTimeout);
        }

        public TEntity Add(TEntity entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            var id = parts.Connection.Insert(entity, parts.Transaction, commandTimeout);

            TryToSetIdentityProperty(entity, id);

            return entity;
        }

        public void Update(TEntity entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            parts.Connection.Update(entity, parts.Transaction, commandTimeout);
        }

        public bool Delete(TEntity entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return parts.Connection.Delete(entity, parts.Transaction, commandTimeout);
        }

        public string GetColumns()
        {
            return ColumnBuilder<TEntity>.GetColumns();
        }

        private static void TryToSetIdentityProperty<T>(T entity, int id)
        {
            var type = entity!.GetType();
            var idProperty = type.GetProperty("Id");
            if (idProperty?.PropertyType == typeof(int) || idProperty?.PropertyType == typeof(long))
            {
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
                            property.SetValue(entity, id);
                        }
                    }
                }
            }
        }

        private static void TryToSetIdentityProperty<T>(T entity, long id)
        {
            var type = entity!.GetType();
            var idProperty = type.GetProperty("Id");
            if (idProperty?.PropertyType == typeof(int) || idProperty?.PropertyType == typeof(long))
            {
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
                            property.SetValue(entity, id);
                        }
                    }
                }
            }
        }
    }
}
