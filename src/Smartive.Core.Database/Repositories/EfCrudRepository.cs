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
        public async Task<IDbContextTransaction> Transaction() =>
            Context.Database.CurrentTransaction ?? await Context.Database.BeginTransactionAsync();

        /// <inheritdoc />
        public virtual IQueryable<TEntity> AsQueryable()
        {
            return Entities.AsQueryable();
        }

        /// <inheritdoc />
        public virtual async Task<IList<TEntity>> GetAll()
        {
            return await Entities.ToListAsync();
        }

        /// <inheritdoc />
        public virtual Task<TEntity> GetById(TKey id)
        {
            return Entities.SingleOrDefaultAsync(e => e.Id.Equals(id));
        }

        /// <inheritdoc />
        public async Task<IList<TResult>> Query<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return await query(AsQueryable()).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<TResult> QuerySingle<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return await query(AsQueryable()).SingleOrDefaultAsync();
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
        public virtual async Task<IList<TEntity>> Create(IEnumerable<TEntity> entities)
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

            await Entities.AddRangeAsync(enumerable);
            await Context.SaveChangesAsync();
            return enumerable;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Update(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            UpdateEntity(entity);

            await Context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc />
        public virtual async Task<IList<TEntity>> Update(IEnumerable<TEntity> entities)
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

            foreach (var entity in enumerable)
            {
                UpdateEntity(entity);
            }

            await Context.SaveChangesAsync();
            return enumerable;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Save(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return await ExistsInDatabase(entity)
                ? await Update(entity)
                : await Create(entity);
        }

        /// <inheritdoc />
        public virtual async Task<IList<TEntity>> Save(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            using (var transaction = await Transaction())
            {
                var result = await Task.WhenAll(entities.Select(Save));
                transaction.Commit();
                return result;
            }
        }

        /// <inheritdoc />
        public async Task<SynchronizationResult<TKey, TEntity>> SynchronizeCollection(
            IQueryable<TEntity> source,
            IEnumerable<TEntity> newEntities,
            bool useTransaction = false)
        {
            var newList = newEntities.ToList();
            var entities = await source.ToListAsync();

            var addEntities = new List<TEntity>();
            var editEntities = new List<TEntity>();

            foreach (var entity in newList)
            {
                if (await ExistsInDatabase(entity))
                {
                    editEntities.Add(entity);
                }
                else
                {
                    addEntities.Add(entity);
                }
            }

            var oldEntities = entities
                .Where(entity => !newList.Any(newEntity => entity.Id.Equals(newEntity.Id)))
                .ToList();

            if (!useTransaction)
            {
                return new SynchronizationResult<TKey, TEntity>
                {
                    Added = await Create(addEntities),
                    Updated = await Update(editEntities),
                    Removed = await Delete(oldEntities)
                };
            }

            using (var transaction = await Transaction())
            {
                var result = new SynchronizationResult<TKey, TEntity>
                {
                    Added = await Create(addEntities),
                    Updated = await Update(editEntities),
                    Removed = await Delete(oldEntities)
                };
                transaction.Commit();
                return result;
            }
        }

        /// <inheritdoc />
        public Task<SynchronizationResult<TKey, TEntity>> SynchronizeCollection(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> source,
            IEnumerable<TEntity> newEntities,
            bool useTransaction = false)
        {
            return SynchronizeCollection(source(AsQueryable()), newEntities, useTransaction);
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
        public async Task<IList<TEntity>> Delete(IEnumerable<TEntity> entities)
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

            foreach (var entity in enumerable)
            {
                await Delete(entity);
            }

            return enumerable;
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
        public async Task<IList<TEntity>> DeleteById(IEnumerable<TKey> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var enumerable = ids.ToList();
            var entities = new List<TEntity>();
            if (enumerable.Count <= 0)
            {
                return new List<TEntity>();
            }

            foreach (var entity in enumerable)
            {
                entities.Add(await DeleteById(entity));
            }

            return entities;
        }

        /// <summary>
        /// Determines if an entity exists in the database context with the given id.
        /// If the id value is the same as the `default` of the type, then it's assumed
        /// to be non existing.
        /// </summary>
        /// <param name="entity">Entity to check.</param>
        /// <returns>True if an entity with the given id is found, false otherwise.</returns>
        protected async Task<bool> ExistsInDatabase(TEntity entity)
        {
            if (EqualityComparer<TKey>.Default.Equals(entity.Id, default))
            {
                return false;
            }

            return await Entities.AnyAsync(e => e.Id.Equals(entity.Id));
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
                localEntity =>
                    localEntity == entity || EqualityComparer<TKey>.Default.Equals(localEntity.Id, entity.Id));
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
            using (var transaction = await Transaction())
            {
                var result = await action();
                transaction.Commit();
                return result;
            }
        }

        private void UpdateEntity(TEntity entity)
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
