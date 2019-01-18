using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Repositories
{
#pragma warning disable SA1402

    /// <inheritdoc />
    /// <summary>
    /// Crud base repository that manages a list of entities in the database context.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TEntity">Type of the entity.</typeparam>
    /// <typeparam name="TContext">Type of the database context.</typeparam>
    public class EfCrudRepository<TKey, TEntity, TContext> : ICrudRepository<TKey, TEntity>
        where TEntity : Base<TKey>
        where TContext : DbContext
    {
        /// <summary>
        /// Create an instance of the repository. Provides basic functionality.
        /// </summary>
        /// <param name="context">Database context.</param>
        public EfCrudRepository(TContext context)
        {
            Context = context;
        }

        /// <summary>
        /// The given database context for this repository.
        /// </summary>
        protected TContext Context { get; }

        /// <summary>
        /// Entity-set of this repository.
        /// </summary>
        protected DbSet<TEntity> Entities => Context.Set<TEntity>();

        /// <inheritdoc />
        public virtual IQueryable<TEntity> AsQueryable()
        {
            return Entities.AsQueryable();
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAll()
        {
            var entities = await Entities.ToListAsync();
            return entities;
        }

        /// <inheritdoc />
        public virtual Task<TEntity> GetById(TKey id)
        {
            return Entities.SingleOrDefaultAsync(e => e.Id.Equals(id));
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Create(TEntity entity)
        {
            await Entities.AddAsync(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> Create(IEnumerable<TEntity> entities)
        {
            var enumerable = entities.ToList();
            if (enumerable.Count <= 0)
            {
                return enumerable;
            }

            await Entities.AddRangeAsync(enumerable);
            await Context.SaveChangesAsync();
            return enumerable;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Update(TEntity entity)
        {
            if (IsTracked(entity, out var tracked))
            {
                Context.Entry(tracked).CurrentValues.SetValues(entity);
            }
            else
            {
                Entities.Update(entity);
            }

            await Context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> Update(IEnumerable<TEntity> entities)
        {
            var enumerable = entities.ToList();
            if (enumerable.Count <= 0)
            {
                return enumerable;
            }

            foreach (var entity in enumerable)
            {
                if (IsTracked(entity, out var tracked))
                {
                    Context.Entry(tracked).CurrentValues.SetValues(entity);
                }
                else
                {
                    Entities.Update(entity);
                }
            }

            await Context.SaveChangesAsync();
            return enumerable;
        }

        /// <inheritdoc />
        public virtual Task<TEntity> Save(TEntity entity)
        {
            return entity.Id.Equals(default)
                ? Create(entity)
                : Update(entity);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> Save(IEnumerable<TEntity> entities)
        {
            var enumerable = entities.ToList();
            return (await Create(enumerable.Where(entity => entity.Id.Equals(default))))
                .Concat(
                    await Update(enumerable.Where(entity => !entity.Id.Equals(default))));
        }

        /// <inheritdoc />
        public async Task<TEntity> Delete(TEntity entity)
        {
            if (!IsTracked(entity, out var trackedEntity))
            {
                return await DeleteById(entity.Id);
            }

            Entities.Remove(trackedEntity);
            await Context.SaveChangesAsync();
            return trackedEntity;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> DeleteById(TKey id)
        {
            var entity = await GetById(id);
            if (entity == null)
            {
                return null;
            }

            Entities.Remove(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Determines if an entity is already tracked. Returns the tracked
        /// entity as an out parameter. If it's not tracked,
        /// the entity should be added before calling `SaveChangesAsync()`.
        /// </summary>
        /// <param name="entity">The entity in question.</param>
        /// <param name="trackedEntity">The tracked entity instance.</param>
        /// <returns>True if the entity is already tracked, false otherwise.</returns>
        protected bool IsTracked(TEntity entity, out TEntity trackedEntity)
        {
            trackedEntity = Entities.Local.SingleOrDefault(
                localEntity => localEntity == entity || localEntity.Id.Equals(entity.Id));
            return trackedEntity != null;
        }
    }

    /// <inheritdoc cref="EfCrudRepository{TKey,TEntity,TContext}" />
    public class EfCrudRepository<TEntity, TContext> : EfCrudRepository<int, TEntity, TContext>,
        ICrudRepository<TEntity>
        where TEntity : Base
        where TContext : DbContext
    {
        /// <summary>
        /// Create an instance of the repository. Provides basic functionality.
        /// </summary>
        /// <param name="context">Database context.</param>
        public EfCrudRepository(TContext context)
            : base(context)
        {
        }
    }

    /// <inheritdoc />
    public class EfCrudRepository<TEntity> : EfCrudRepository<TEntity, DbContext>
        where TEntity : Base
    {
        /// <summary>
        /// Create an instance of the repository. Provides basic functionality.
        /// </summary>
        /// <param name="context">Database context.</param>
        public EfCrudRepository(DbContext context)
            : base(context)
        {
        }
    }

#pragma warning restore SA1402
}
