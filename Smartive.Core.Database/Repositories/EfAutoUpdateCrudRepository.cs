using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Attributes;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Repositories
{
#pragma warning disable SA1402

    /// <summary>
    /// Repository that uses the <see cref="AutoUpdatable"/> attribute to update entites.
    /// </summary>
    /// <typeparam name="TKey">Type of the id property.</typeparam>
    /// <typeparam name="TEntity">Type of the entity.</typeparam>
    public class EfAutoUpdateCrudRepository<TKey, TEntity> : EfCrudBaseRepository<TKey, TEntity>
        where TEntity : Base<TKey>
    {
        /// <inheritdoc />
        public EfAutoUpdateCrudRepository(DbSet<TEntity> entities, DbContext context)
            : base(entities, context)
        {
        }

        /// <inheritdoc />
        public override async Task<TEntity> Update(TEntity entity)
        {
            var dbEntity = await GetById(entity.Id);
            if (dbEntity == null)
            {
                throw new KeyNotFoundException();
            }

            var type = entity.GetType();

            if (type.GetCustomAttribute<AutoUpdatable>() == null &&
                type.GetProperties().All(property => property.GetCustomAttribute<AutoUpdatable>() == null))
            {
                throw new NoAutoUpdatableElementsFound(type);
            }

            var properties = entity
                .GetType()
                .GetProperties()
                .Where(property => property.CanRead && property.CanWrite);

            if (type.GetCustomAttribute<AutoUpdatable>() == null)
            {
                properties = properties.Where(property => property.CanRead &&
                                                          property.CanWrite &&
                                                          property.GetCustomAttribute<AutoUpdatable>() != null);
            }

            foreach (var property in properties)
            {
                property.SetValue(
                    dbEntity,
                    property.GetValue(entity));
            }

            await Context.SaveChangesAsync();
            return dbEntity;
        }
    }

    /// <inheritdoc />
    public class EfAutoUpdateCrudRepository<TEntity> : EfAutoUpdateCrudRepository<int, TEntity>
        where TEntity : Base
    {
        /// <inheritdoc />
        public EfAutoUpdateCrudRepository(DbSet<TEntity> entities, DbContext context)
            : base(entities, context)
        {
        }
    }

#pragma warning restore SA1402
}
