using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Smartive.Core.Database.Models;
using Smartive.Core.Database.Repositories;

namespace Smartive.Core.Database.Extensions
{
    /// <summary>
    /// Contains extension methods for the service collection to add auto repositories
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a repository of the given entity to the service collection with the given scope. The repository
        /// is registered with the type <see cref="ICrudRepository{TEntity}"/>.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> that should register.</param>
        /// <param name="lifetime">ServiceLifetime</param>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TContext">The type of the db context.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> for chaining</returns>
        public static IServiceCollection AddRepository<TEntity, TContext>(
            this IServiceCollection collection,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : Base
            where TContext : DbContext
        {
            collection.Add(
                new ServiceDescriptor(
                    typeof(ICrudRepository<TEntity>),
                    provider => new EfCrudRepository<TEntity, TContext>(provider.GetService<TContext>()),
                    lifetime));

            return collection;
        }

        /// <summary>
        /// Adds a repository of the given entity to the service collection with the given scope. The repository
        /// is registered with the type <see cref="ICrudRepository{TKey,TEntity}"/>.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> that should register.</param>
        /// <param name="lifetime">ServiceLifetime</param>
        /// <typeparam name="TKey">The type of the entity id.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TContext">The type of the db context.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> for chaining</returns>
        public static IServiceCollection AddRepository<TKey, TEntity, TContext>(
            this IServiceCollection collection,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : Base<TKey>
            where TContext : DbContext
        {
            collection.Add(
                new ServiceDescriptor(
                    typeof(ICrudRepository<TKey, TEntity>),
                    provider => new EfCrudRepository<TKey, TEntity, TContext>(provider.GetService<TContext>()),
                    lifetime));

            return collection;
        }
    }
}
