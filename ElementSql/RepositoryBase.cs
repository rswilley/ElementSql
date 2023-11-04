using Dapper.Contrib.Extensions;
using ElementSql.Interfaces;

namespace ElementSql
{
    public abstract class RepositoryBase<TEntity> : QueryBase where TEntity : class
    {
        public async Task<TEntity> GetById(object id, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.GetAsync<TEntity>(id, parts.Transaction, commandTimeout);
        }

        public async Task<IEnumerable<TEntity>> GetAll(IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.GetAllAsync<TEntity>(parts.Transaction, commandTimeout);
        }

        public async Task<TEntity> Add(TEntity entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            var id = await parts.Connection.InsertAsync(entity, parts.Transaction, commandTimeout);

            TryToSetIdentityProperty();

            return entity;

            void TryToSetIdentityProperty()
            {
                var type = entity.GetType();
                var idProperty = type.GetProperty("Id");
                if (idProperty?.PropertyType == typeof(int) || idProperty?.PropertyType == typeof(long))
                {
                    idProperty.SetValue(entity, id);
                } else
                {
                    var properties = type.GetProperties();
                    foreach (var property in properties )
                    {
                        var attributes = property.GetCustomAttributes(true);
                        foreach (var attribute in attributes) {
                            if (attribute is KeyAttribute)
                            {
                                property.SetValue(entity, id);
                            }
                        }
                    }
                }
            }
        }

        public async Task Update(TEntity entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            await parts.Connection.UpdateAsync(entity, parts.Transaction, commandTimeout);
        }

        public async Task<bool> Delete(TEntity entity, IConnectionContext context, int? commandTimeout = null)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.DeleteAsync(entity, parts.Transaction, commandTimeout);
        }

        public string GetColumns()
        {
            return ColumnBuilder<TEntity>.GetColumns();
        }
    }
}
