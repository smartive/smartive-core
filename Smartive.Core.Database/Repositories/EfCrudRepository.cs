using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Repositories
{
#pragma warning disable SA1402

    /// <inheritdoc />
    public abstract class EfCrudRepository<TKey, TEntity, TContext> : EfCrudBaseRepository<TKey, TEntity, TContext>
        where TEntity : Base<TKey>
        where TContext : DbContext
    {
        /// <inheritdoc />
        protected EfCrudRepository(DbSet<TEntity> entities, TContext context)
            : base(entities, context)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Updates the given entity via the UpdateEntity method.
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>Task with updated entity</returns>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Exception when entity with the given key was not found.</exception>
        public override async Task<TEntity> Update(TEntity entity)
        {
            var dbEntity = await GetById(entity.Id);
            if (dbEntity == null)
            {
                throw new KeyNotFoundException();
            }

            UpdateEntity(ref dbEntity, entity);

            await Context.SaveChangesAsync();
            return dbEntity;
        }

        /// <summary>
        /// Method that updates the actual database entity with the values of entity.
        /// </summary>
        /// <param name="dbEntity">The database entity to update (ref param)</param>
        /// <param name="entity">The updated entity that overrides the values</param>
        protected abstract void UpdateEntity(ref TEntity dbEntity, TEntity entity);
    }

    /// <inheritdoc />
    public abstract class EfCrudRepository<TEntity, TContext> : EfCrudRepository<int, TEntity, TContext>, ICrudRepository<TEntity>
        where TEntity : Base
        where TContext : DbContext
    {
        /// <inheritdoc />
        protected EfCrudRepository(DbSet<TEntity> entities, TContext context)
            : base(entities, context)
        {
        }
    }

    /// <inheritdoc />
    public abstract class EfCrudRepository<TEntity> : EfCrudRepository<TEntity, DbContext>
        where TEntity : Base
    {
        /// <inheritdoc />
        protected EfCrudRepository(DbSet<TEntity> entities, DbContext context)
            : base(entities, context)
        {
        }
    }

#pragma warning restore SA1402
}
