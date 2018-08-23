using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Repositories
{
#pragma warning disable SA1402

    /// <summary>
    /// Repository interface that uses default actions to CRUD for an entity.
    /// </summary>
    /// <typeparam name="TKey">Type of the key (id).</typeparam>
    /// <typeparam name="TEntity">Type of the entity.</typeparam>
    public interface ICrudRepository<in TKey, TEntity>
        where TEntity : Base<TKey>
    {
        /// <summary>
        /// Returns the whole list of entites as a queryable.
        /// </summary>
        /// <returns>A queryable list of entities.</returns>
        IQueryable<TEntity> AsQueryable();

        /// <summary>
        /// Returns all entities (select * from ...).
        /// </summary>
        /// <returns>An enumerable list of all entities.</returns>
        Task<IEnumerable<TEntity>> GetAll();

        /// <summary>
        /// Return an entity by it's key.
        /// </summary>
        /// <param name="id">The given Id to search.</param>
        /// <returns>The entity, or default(Type) if no entity is found.</returns>
        Task<TEntity> GetById(TKey id);

        /// <summary>
        /// Create the given entity in the context.
        /// </summary>
        /// <param name="entity">Entity to save.</param>
        /// <returns>The updated entity with the set Id.</returns>
        Task<TEntity> Create(TEntity entity);

        /// <summary>
        /// Update the given entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>The updated entity.</returns>
        Task<TEntity> Update(TEntity entity);

        /// <summary>
        /// Delete a given entity by it's key.
        /// </summary>
        /// <param name="id">The key to delete.</param>
        /// <returns>A task that resolves to the entity when the element is deleted.</returns>
        Task<TEntity> DeleteById(TKey id);
    }

    /// <inheritdoc />
    public interface ICrudRepository<TEntity> : ICrudRepository<int, TEntity>
        where TEntity : Base
    {
    }

#pragma warning restore SA1402
}
