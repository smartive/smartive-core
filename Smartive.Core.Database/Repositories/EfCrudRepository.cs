using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        public Task<IDbContextTransaction> BeginTransaction() => Context.Database.BeginTransactionAsync();

        /// <inheritdoc />
        public virtual IQueryable<TEntity> AsQueryable()
        {
            return Entities.AsQueryable();
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAll()
        {
            return await Entities.ToListAsync();
        }

        /// <inheritdoc />
        public virtual Task<TEntity> GetById(TKey id)
        {
            return Entities.SingleOrDefaultAsync(e => e.Id.Equals(id));
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Create(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await Entities.AddAsync(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> Create(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var enumerable = entities.ToList();
            if (enumerable.Count <= 0)
            {
                return enumerable;
            }

            return await ExecuteTransactional(() => Task.WhenAll(enumerable.Select(Create)));
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Update(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

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
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var enumerable = entities.ToList();
            if (enumerable.Count <= 0)
            {
                return enumerable;
            }

            return await ExecuteTransactional(() => Task.WhenAll(enumerable.Select(Update)));
        }

        /// <inheritdoc />
        public virtual Task<TEntity> Save(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return entity.Id.Equals(default(TKey))
                ? Create(entity)
                : Update(entity);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> Save(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            using (var transaction = await Context.Database.BeginTransactionAsync())
            {
                var result = await Task.WhenAll(entities.Select(Save));
                transaction.Commit();
                return result;
            }
        }

        /// <inheritdoc />
        public Task<SynchronizationResult<TKey, TEntity>> SynchronizeCollection(
            IQueryable<TEntity> source,
            IEnumerable<TEntity> newEntities,
            bool useTransaction = false)
        {
            var newList = newEntities.ToList();

            async Task<SynchronizationResult<TKey, TEntity>> Synchronization()
            {
                var entities = await source.ToListAsync();

                var addEntities = newList
                    .Where(entity => entity.Id.Equals(default(TKey)))
                    .ToList();
                var existingEntities = newList
                    .Where(entity => !entity.Id.Equals(default(TKey)))
                    .ToList();
                var oldEntities = entities
                    .Where(entity => !newList.Any(newEntity => entity.Id.Equals(newEntity.Id)))
                    .ToList();

                return new SynchronizationResult<TKey, TEntity>
                {
                    Added = useTransaction
                        ? await Create(addEntities)
                        : (await Task.WhenAll(addEntities.Select(Create))).ToList(),
                    Updated = useTransaction
                        ? await Update(existingEntities)
                        : (await Task.WhenAll(existingEntities.Select(Update))).ToList(),
                    Removed = useTransaction
                        ? await Delete(oldEntities)
                        : (await Task.WhenAll(oldEntities.Select(Delete))).ToList()
                };
            }

            return useTransaction
                ? ExecuteTransactional(Synchronization)
                : Synchronization();
        }

        /// <inheritdoc />
        public async Task<TEntity> Delete(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!IsTracked(entity, out var trackedEntity))
            {
                return await DeleteById(entity.Id);
            }

            Entities.Remove(trackedEntity);
            await Context.SaveChangesAsync();
            return trackedEntity;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> Delete(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var enumerable = entities.ToList();
            if (enumerable.Count <= 0)
            {
                return enumerable;
            }

            return await ExecuteTransactional(() => Task.WhenAll(enumerable.Select(Delete)));
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

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> DeleteById(IEnumerable<TKey> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var enumerable = ids.ToList();
            if (enumerable.Count <= 0)
            {
                return new List<TEntity>();
            }

            return await ExecuteTransactional(() => Task.WhenAll(enumerable.Select(DeleteById)));
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

        /// <summary>
        /// Executes an action (or therefore multiple things) in a transaction.
        /// </summary>
        /// <param name="action">The action that should be performed.</param>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <returns>A task that resolves to the result.</returns>
        protected async Task<TResult> ExecuteTransactional<TResult>(Func<Task<TResult>> action)
        {
            using (var transaction = await Context.Database.BeginTransactionAsync())
            {
                var result = await action();
                transaction.Commit();
                return result;
            }
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
