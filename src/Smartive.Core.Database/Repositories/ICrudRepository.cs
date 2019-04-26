using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Repositories
{
#pragma warning disable SA1402

    /// <summary>
    /// Repository interface that uses default actions to CRUD for an entity.
    /// </summary>
    /// <typeparam name="TKey">Type of the key (id).</typeparam>
    /// <typeparam name="TEntity">Type of the entity.</typeparam>
    public interface ICrudRepository<TKey, TEntity>
        where TEntity : Base<TKey>
    {
        /// <summary>
        /// Starts a transaction on the given db context.
        /// Be advised, that a transaction can be "used" by other repositories, but they
        /// must use the same DbContext.
        /// </summary>
        /// <example>
        /// using(var transaction = await repository.BeginTransaction()){
        ///    repository.Save(entity);
        ///    transaction.Commit();
        /// }
        /// </example>
        /// <returns>A database transaction that should be committed.</returns>
        Task<IDbContextTransaction> BeginTransaction();

        /// <summary>
        /// Returns the whole list of entities as a queryable.
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
        /// <returns>The created entity with the set Id.</returns>
        Task<TEntity> Create(TEntity entity);

        /// <summary>
        /// Create the given entities in the context.
        /// </summary>
        /// <param name="entities">Entities to save.</param>
        /// <returns>The created entities with the set Ids.</returns>
        Task<IEnumerable<TEntity>> Create(IEnumerable<TEntity> entities);

        /// <summary>
        /// Update the given entity. This updates all properties on the entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>The updated entity.</returns>
        Task<TEntity> Update(TEntity entity);

        /// <summary>
        /// Update the given entities. This updates all properties on the entities.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        /// <returns>The updated entities.</returns>
        Task<IEnumerable<TEntity>> Update(IEnumerable<TEntity> entities);

        /// <summary>
        /// Saves the given entity. Save means, `if Id == default then create else update`.
        /// Navigation properties are not saved.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <returns>The saved entity.</returns>
        Task<TEntity> Save(TEntity entity);

        /// <summary>
        /// Saves the given entities. Save means, `if Id == default then create else update`.
        /// Navigation properties are not saved.
        /// </summary>
        /// <param name="entities">The entities to save.</param>
        /// <returns>The saved entities.</returns>
        Task<IEnumerable<TEntity>> Save(IEnumerable<TEntity> entities);

        /// <summary>
        /// Synchronizes a given list of entities (defined by the <paramref name="source"/> query)
        /// with a given list of entities. This means, that the query is executed, then all entities
        /// that are in the source but not in the given list, are deleted. All other elements are
        /// created or updated according to their `Id` field.
        /// </summary>
        /// <param name="source">The source query for the synchronization.</param>
        /// <param name="newEntities">The "master" list that will overwrite the source.</param>
        /// <param name="useTransaction">If true, the sync collection should use a transaction to perform the sync.</param>
        /// <returns>The updated list of entities.</returns>
        Task<SynchronizationResult<TKey, TEntity>> SynchronizeCollection(
            IQueryable<TEntity> source,
            IEnumerable<TEntity> newEntities,
            bool useTransaction = false);

        /// <summary>
        /// Synchronizes a given list of entities (defined by the <paramref name="source"/>)
        /// with a given list of entities. This means, that the query is executed, then all entities
        /// that are in the source but not in the given list, are deleted. All other elements are
        /// created or updated according to their `Id` field.
        /// </summary>
        /// <param name="source">
        /// A function that receives the whole collection as <see cref="IQueryable{T}"/>
        /// and should return a <see cref="IQueryable{T}"/> that is executed.
        /// </param>
        /// <param name="newEntities">The "master" list that will overwrite the source.</param>
        /// <param name="useTransaction">If true, the sync collection should use a transaction to perform the sync.</param>
        /// <returns>The updated list of entities.</returns>
        Task<SynchronizationResult<TKey, TEntity>> SynchronizeCollection(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> source,
            IEnumerable<TEntity> newEntities,
            bool useTransaction = false);

        /// <summary>
        /// Deletes a given entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>The deleted entity.</returns>
        Task<TEntity> Delete(TEntity entity);

        /// <summary>
        /// Deletes the given entities.
        /// </summary>
        /// <param name="entity">The entities to delete.</param>
        /// <returns>The deleted entities.</returns>
        Task<IEnumerable<TEntity>> Delete(IEnumerable<TEntity> entity);

        /// <summary>
        /// Delete a given entity by it's key.
        /// </summary>
        /// <param name="id">The key to delete.</param>
        /// <returns>A task that resolves to the entity when the element is deleted.</returns>
        Task<TEntity> DeleteById(TKey id);

        /// <summary>
        /// Delete the given entities by it's keys.
        /// </summary>
        /// <param name="id">The keys to delete.</param>
        /// <returns>A task that resolves to the entities when the elements are deleted.</returns>
        Task<IEnumerable<TEntity>> DeleteById(IEnumerable<TKey> id);
    }

    /// <inheritdoc />
    public interface ICrudRepository<TEntity> : ICrudRepository<int, TEntity>
        where TEntity : Base
    {
    }

#pragma warning restore SA1402
}
