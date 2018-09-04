using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Repositories
{
#pragma warning disable SA1402

    /// <inheritdoc />
    /// <summary>
    /// Crud base repository that manages a list of entites in the database context.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TEntity">Type of the entity.</typeparam>
    /// <typeparam name="TContext">Type of the database context.</typeparam>
    public abstract class EfCrudBaseRepository<TKey, TEntity, TContext> : ICrudRepository<TKey, TEntity>
        where TEntity : Base<TKey>
        where TContext : DbContext
    {
        /// <summary>
        /// The given database context for this repository.
        /// </summary>
        protected readonly TContext Context;

        /// <summary>
        /// Entityset of this repository.
        /// </summary>
        protected readonly DbSet<TEntity> Entities;

        /// <summary>
        /// Create an instance of the repository. Provides basic functionality.
        /// </summary>
        /// <param name="entities">List of entities.</param>
        /// <param name="context">Database context.</param>
        protected EfCrudBaseRepository(DbSet<TEntity> entities, TContext context)
        {
            Entities = entities;
            Context = context;
        }

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
        public abstract Task<TEntity> Update(TEntity entity);

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
    }

    /// <inheritdoc />
    public abstract class EfCrudBaseRepository<TEntity, TContext> : EfCrudBaseRepository<int, TEntity, TContext>, ICrudRepository<TEntity>
        where TEntity : Base
        where TContext : DbContext
    {
        /// <inheritdoc />
        /// <summary>
        /// Create an instance of the repository. Provides basic functionality.
        /// </summary>
        /// <param name="entities">List of entities.</param>
        /// <param name="context">Database context.</param>
        protected EfCrudBaseRepository(DbSet<TEntity> entities, TContext context)
            : base(entities, context)
        {
        }
    }

    /// <inheritdoc />
    public abstract class EfCrudBaseRepository<TEntity> : EfCrudBaseRepository<TEntity, DbContext>
        where TEntity : Base
    {
        /// <inheritdoc />
        /// <summary>
        /// Create an instance of the repository. Provides basic functionality.
        /// </summary>
        /// <param name="entities">List of entities.</param>
        /// <param name="context">Database context.</param>
        protected EfCrudBaseRepository(DbSet<TEntity> entities, DbContext context)
            : base(entities, context)
        {
        }
    }

#pragma warning restore SA1402
}
