using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Attributes;
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
        where TKey : notnull
        where TEntity : notnull, Base<TKey>
        where TContext : notnull, DbContext
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

        private static EqualityComparer<TKey> KeyComparer => EqualityComparer<TKey>.Default;

        /// <inheritdoc />
        public virtual IQueryable<TEntity> AsQueryable() => Entities.AsQueryable();

        /// <inheritdoc />
        public virtual async Task<IList<TEntity>> GetAll() => await AsQueryable().ToListAsync();

        /// <inheritdoc />
        public virtual async Task<TEntity?> GetById(TKey id) =>
            await AsQueryable().SingleOrDefaultAsync(e => e.Id.Equals(id));

        /// <inheritdoc />
        public virtual async Task<TEntity> Create(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await CreateEntities(new[] { entity });
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
            if (!enumerable.Any())
            {
                return enumerable;
            }

            await CreateEntities(enumerable);
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

            UpdateEntities(new[] { entity });
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
            if (!enumerable.Any())
            {
                return enumerable;
            }

            UpdateEntities(enumerable);
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

            await SaveEntities(new[] { entity });
            await Context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc />
        public virtual async Task<IList<TEntity>> Save(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var list = entities.ToList();
            await SaveEntities(list);
            await Context.SaveChangesAsync();
            return list;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity?> Delete(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var deleted = await DeleteEntities(new[] { entity });
            await Context.SaveChangesAsync();

            return deleted.FirstOrDefault();
        }

        /// <inheritdoc />
        public virtual async Task<IList<TEntity>> Delete(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var enumerable = entities.ToList();
            if (!enumerable.Any())
            {
                return enumerable;
            }

            var deleted = await DeleteEntities(enumerable);
            await Context.SaveChangesAsync();

            return deleted;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity?> DeleteById(TKey id)
        {
            var deleted = await DeleteEntities(new[] { id });
            await Context.SaveChangesAsync();

            return deleted.FirstOrDefault();
        }

        /// <inheritdoc />
        public virtual async Task<IList<TEntity>> DeleteById(IEnumerable<TKey> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var enumerable = ids.ToList();
            if (!enumerable.Any())
            {
                return new List<TEntity>();
            }

            var deleted = await DeleteEntities(enumerable);
            await Context.SaveChangesAsync();

            return deleted;
        }

        /// <inheritdoc />
        public virtual async Task<SynchronizationResult<TKey, TEntity>> SynchronizeCollection(
            IQueryable<TEntity> sourceList,
            IEnumerable<TEntity> newEntities)
        {
            var newList = newEntities.ToList();
            var entities = await sourceList.ToListAsync();

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

            await CreateEntities(addEntities);
            UpdateEntities(editEntities);
            var oldEntities = await DeleteEntities(
                entities
                    .Where(entity => !newList.Any(newEntity => entity.Id.Equals(newEntity.Id))));

            await Context.SaveChangesAsync();

            return new SynchronizationResult<TKey, TEntity>
            {
                Added = addEntities,
                Updated = editEntities,
                Removed = oldEntities,
            };
        }

        /// <inheritdoc />
        public virtual Task<SynchronizationResult<TKey, TEntity>> SynchronizeCollection(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> sourceList,
            IEnumerable<TEntity> newEntities) =>
            SynchronizeCollection(sourceList(AsQueryable()), newEntities);

        /// <inheritdoc />
        public virtual Task<SynchronizationResult<TKey, TEntity>> SynchronizeCollection(
            IEnumerable<TEntity> newEntities) =>
            SynchronizeCollection(AsQueryable(), newEntities);

        /// <summary>
        /// Determines if an entity exists in the database context with the given id.
        /// If the id value is the same as the `default` of the type, then it's assumed
        /// to be non existing.
        /// </summary>
        /// <param name="entity">Entity to check.</param>
        /// <returns>True if an entity with the given id is found, false otherwise.</returns>
        protected async Task<bool> ExistsInDatabase(TEntity entity)
        {
            if (IsTracked(entity, out _))
            {
                return true;
            }

            if (KeyComparer.Equals(entity.Id, default!))
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
        protected bool IsTracked(TEntity entity, out TEntity? trackedEntity) => IsTracked(
            localEntity => localEntity == entity || KeyComparer.Equals(localEntity.Id, entity.Id),
            out trackedEntity);

        /// <summary>
        /// Determines if an entity is already tracked. Returns the tracked
        /// entity as an out parameter. If it's not tracked,
        /// the entity should be added before calling `SaveChangesAsync()`.
        /// </summary>
        /// <param name="predicate">The predicate to search for the entity.</param>
        /// <param name="trackedEntity">The tracked entity instance.</param>
        /// <returns>True if the entity is already tracked, false otherwise.</returns>
        protected bool IsTracked(Func<TEntity, bool> predicate, out TEntity? trackedEntity)
        {
            trackedEntity = Entities.Local.SingleOrDefault(predicate);
            return trackedEntity != null;
        }

        private Task CreateEntities(IEnumerable<TEntity> entities) => Entities.AddRangeAsync(entities);

        private void UpdateEntities(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (IsTracked(entity, out var tracked))
                {
                    if (tracked == null)
                    {
                        continue;
                    }

                    Context.Entry(tracked).CurrentValues.SetValues(entity);
                    SetIgnoredOnUpdate(tracked);
                }
                else
                {
                    Entities.Update(entity);
                    SetIgnoredOnUpdate(entity);
                }
            }
        }

        private async Task SaveEntities(IEnumerable<TEntity> entities)
        {
            var newEntities = new List<TEntity>();
            var existingEntities = new List<TEntity>();

            foreach (var entity in entities)
            {
                if (await ExistsInDatabase(entity))
                {
                    existingEntities.Add(entity);
                }
                else
                {
                    newEntities.Add(entity);
                }
            }

            await CreateEntities(newEntities);
            UpdateEntities(existingEntities);
        }

        private Task<IList<TEntity>> DeleteEntities(IEnumerable<TEntity> entities) =>
            DeleteEntities(entities.Select(e => e.Id));

        private async Task<IList<TEntity>> DeleteEntities(IEnumerable<TKey> entityIds)
        {
            var deleted = new List<TEntity>();

            foreach (var id in entityIds)
            {
                TEntity? entity;
                if (IsTracked(e => KeyComparer.Equals(e.Id, id), out var trackedEntity))
                {
                    entity = trackedEntity;
                }
                else
                {
                    entity = await GetById(id);
                }

                if (entity != null)
                {
                    deleted.Add(entity);
                    Entities.Remove(entity);
                }
            }

            return deleted;
        }

        private void SetIgnoredOnUpdate(TEntity entity)
        {
            foreach (var property in entity.GetType()
                .GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(IgnoreOnUpdateAttribute))))
            {
                Context.Entry(entity).Property(property.Name).IsModified = false;
            }
        }
    }

    /// <inheritdoc cref="EfCrudRepository{TKey,TEntity,TContext}" />
    public class EfCrudRepository<TEntity, TContext> : EfCrudRepository<int, TEntity, TContext>,
        ICrudRepository<TEntity>
        where TEntity : notnull, Base
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
        where TEntity : notnull, Base
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
