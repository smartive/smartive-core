using System.Collections.Generic;
using System.Linq;

namespace Smartive.Core.Database.Models
{
    /// <summary>
    /// Result that is calculated for the `SynchronizeCollection` method of a CrudRepository.
    /// </summary>
    /// <typeparam name="TKey">Key type of the entity.</typeparam>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public class SynchronizationResult<TKey, TEntity>
        where TKey : notnull
        where TEntity : Base<TKey>
    {
        /// <summary>
        /// Collection of all entities that were added during the `SynchronizeCollection` method.
        /// </summary>
        public IEnumerable<TEntity> Added { get; set; } = new List<TEntity>();

        /// <summary>
        /// Collection of all entities that were updated during the `SynchronizeCollection` method.
        /// </summary>
        public IEnumerable<TEntity> Updated { get; set; } = new List<TEntity>();

        /// <summary>
        /// Collection of all entities that were removed during the `SynchronizeCollection` method.
        /// </summary>
        public IEnumerable<TEntity> Removed { get; set; } = new List<TEntity>();

        /// <summary>
        /// Collection of all entities that are present (added or updated, and not deleted) after
        /// the `SynchronizeCollection` method.
        /// </summary>
        public IEnumerable<TEntity> Synchronized => Added.Concat(Updated);
    }
}
