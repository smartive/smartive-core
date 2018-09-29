using System;
using System.Linq;
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
        /// <param name="entities">A function that returns a reference to a DbSet of the given entity type.</param>
        /// <param name="lifetime">ServiceLifetime</param>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TContext">The type of the db context.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> for chaining</returns>
        public static IServiceCollection AddAutoRepository<TEntity, TContext>(
            this IServiceCollection collection,
            Func<TContext, DbSet<TEntity>> entities,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : Base
            where TContext : DbContext
        {
            ICrudRepository<TEntity> ProviderFunction(IServiceProvider provider)
            {
                var context = provider.GetService<TContext>();
                return new EfAutoUpdateCrudRepository<TEntity, TContext>(entities(context), context);
            }

            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    collection.AddSingleton(ProviderFunction);
                    break;
                case ServiceLifetime.Transient:
                    collection.AddTransient(ProviderFunction);
                    break;
                default:
                    collection.AddScoped(ProviderFunction);
                    break;
            }

            return collection;
        }

        /// <summary>
        /// Adds a repository of the given entity to the service collection with the given scope. The repository
        /// is registered with the type <see cref="ICrudRepository{TKey,TEntity}"/>.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> that should register.</param>
        /// <param name="entities">A function that returns a reference to a DbSet of the given entity type.</param>
        /// <param name="lifetime">ServiceLifetime</param>
        /// <typeparam name="TKey">The type of the entity id.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TContext">The type of the db context.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> for chaining</returns>
        public static IServiceCollection AddAutoRepository<TKey, TEntity, TContext>(
            this IServiceCollection collection,
            Func<TContext, DbSet<TEntity>> entities,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : Base<TKey>
            where TContext : DbContext
        {
            ICrudRepository<TKey, TEntity> ProviderFunction(IServiceProvider provider)
            {
                var context = provider.GetService<TContext>();
                return new EfAutoUpdateCrudRepository<TKey, TEntity, TContext>(entities(context), context);
            }

            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    collection.AddSingleton(ProviderFunction);
                    break;
                case ServiceLifetime.Transient:
                    collection.AddTransient(ProviderFunction);
                    break;
                default:
                    collection.AddScoped(ProviderFunction);
                    break;
            }

            return collection;
        }

        /// <summary>
        /// Adds a repository of the given entity to the service collection with the given scope. The repository
        /// is registered with the type <see cref="ICrudRepository{TKey,TEntity}"/>. Does search for a
        /// <see cref="DbSet{TEntity}"/> with reflection.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> that should register.</param>
        /// <param name="lifetime">ServiceLifetime</param>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TContext">The type of the db context.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> for chaining</returns>
        public static IServiceCollection AddAutoRepository<TEntity, TContext>(
            this IServiceCollection collection,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : Base
            where TContext : DbContext
        {
            ICrudRepository<TEntity> ProviderFunction(IServiceProvider provider)
            {
                var context = provider.GetService<TContext>();

                var property = context
                    .GetType()
                    .GetProperties()
                    .Where(p => p.CanRead && p.CanWrite)
                    .First(p => p.PropertyType.IsAssignableFrom(typeof(DbSet<TEntity>)));

                var entities = property.GetValue(context) as DbSet<TEntity>;

                return new EfAutoUpdateCrudRepository<TEntity, TContext>(entities, context);
            }

            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    collection.AddSingleton(ProviderFunction);
                    break;
                case ServiceLifetime.Transient:
                    collection.AddTransient(ProviderFunction);
                    break;
                default:
                    collection.AddScoped(ProviderFunction);
                    break;
            }

            return collection;
        }

        /// <summary>
        /// Adds a repository of the given entity to the service collection with the given scope. The repository
        /// is registered with the type <see cref="ICrudRepository{TKey,TEntity}"/>. Does search for a
        /// <see cref="DbSet{TEntity}"/> with reflection.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> that should register.</param>
        /// <param name="lifetime">ServiceLifetime</param>
        /// <typeparam name="TKey">The type of the entity id.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TContext">The type of the db context.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> for chaining</returns>
        public static IServiceCollection AddAutoRepository<TKey, TEntity, TContext>(
            this IServiceCollection collection,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : Base<TKey>
            where TContext : DbContext
        {
            ICrudRepository<TKey, TEntity> ProviderFunction(IServiceProvider provider)
            {
                var context = provider.GetService<TContext>();

                var property = context
                    .GetType()
                    .GetProperties()
                    .Where(p => p.CanRead && p.CanWrite)
                    .First(p => p.PropertyType.IsAssignableFrom(typeof(DbSet<TEntity>)));

                var entities = property.GetValue(context) as DbSet<TEntity>;

                return new EfAutoUpdateCrudRepository<TKey, TEntity, TContext>(entities, context);
            }

            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    collection.AddSingleton(ProviderFunction);
                    break;
                case ServiceLifetime.Transient:
                    collection.AddTransient(ProviderFunction);
                    break;
                default:
                    collection.AddScoped(ProviderFunction);
                    break;
            }

            return collection;
        }
    }
}
