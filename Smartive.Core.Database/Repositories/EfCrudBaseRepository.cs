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
    public abstract class EfCrudBaseRepository<TKey, TEntity> : ICrudRepository<TKey, TEntity>
        where TEntity : Base<TKey>
    {
        /// <summary>
        /// The given database context for this repository.
        /// </summary>
        protected readonly DbContext Context;

        /// <summary>
        /// Entityset of this repository.
        /// </summary>
        protected readonly DbSet<TEntity> Entities;

        /// <summary>
        /// Create an instance of the repository. Provides basic functionality.
        /// </summary>
        /// <param name="entities">List of entities.</param>
        /// <param name="context">Database context.</param>
        protected EfCrudBaseRepository(DbSet<TEntity> entities, DbContext context)
        {
            Entities = entities;
            Context = context;
        }

        /// <inheritdoc />
        public IQueryable<TEntity> AsQueryable()
        {
            return Entities.AsQueryable();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetAll()
        {
            var entities = await Entities.ToListAsync();
            return entities;
        }

        /// <inheritdoc />
        public Task<TEntity> GetById(TKey id)
        {
            return Entities.SingleOrDefaultAsync(e => e.Id.Equals(id));
        }

        /// <inheritdoc />
        public async Task<TEntity> Create(TEntity entity)
        {
            await Entities.AddAsync(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc />
        public abstract Task<TEntity> Update(TEntity entity);

        /// <inheritdoc />
        public async Task<TEntity> DeleteById(TKey id)
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
    public abstract class EfCrudBaseRepository<TEntity> : EfCrudBaseRepository<int, TEntity>
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
