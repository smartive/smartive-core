using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Smartive.Core.Database.Models;
using Smartive.Core.Database.Repositories;

namespace Smartive.Core.Database.Extensions
{
    /// <summary>
    /// Extensions for the base models.
    /// </summary>
    public static class ModelExtensions
    {
        private const string CreateMethod = "Create";
        private const string UpdateMethod = "Update";
        private const string SaveMethod = "Save";
        private const string DeleteMethod = "Delete";

        private static IServiceProvider _provider;

        /// <summary>
        /// Dictionary with mapping for the model types to their repositories.
        /// </summary>
        internal static Dictionary<Type, Type> ModelRepositories { get; } = new Dictionary<Type, Type>();

        /// <summary>
        /// Resets the model registrations (Repositories) and does
        /// clear the IServiceProvider variable.
        /// </summary>
        public static void ResetRegistrations()
        {
            ModelRepositories.Clear();
            _provider = null;
        }

        /// <summary>
        /// Enable `ModelExtensions` for all base models.
        /// This reference (to <see cref="IServiceProvider"/>) must be set to enable dependency injection.
        /// </summary>
        /// <param name="provider">The ServiceProvider to use.</param>
        /// <returns>The given SerivceProvider.</returns>
        public static IServiceProvider UseModelExtensions(this IServiceProvider provider) => _provider = provider;

        /// <summary>
        /// Create the given model with the associated repository.
        /// </summary>
        /// <param name="model">Model to create.</param>
        /// <typeparam name="TKey">Id type of the model.</typeparam>
        /// <returns>Task that completes when the model is created.</returns>
        public static Task Create<TKey>(this Base<TKey> model) => Execute(CreateMethod, model);

        /// <summary>
        /// Create the given list of models with the associated repository.
        /// </summary>
        /// <param name="entities">Models to create.</param>
        /// <typeparam name="TKey">Id type of the model.</typeparam>
        /// <returns>Task that completes when the models are created.</returns>
        public static Task Create<TKey>(this IEnumerable<Base<TKey>> entities) => Execute(
            CreateMethod,
            entities,
            entities.GetType().GenericTypeArguments.First());

        /// <summary>
        /// Update the given model with the associated repository.
        /// </summary>
        /// <param name="model">Model to update.</param>
        /// <typeparam name="TKey">Id type of the model.</typeparam>
        /// <returns>Task that completes when the model is updated.</returns>
        public static Task Update<TKey>(this Base<TKey> model) => Execute(UpdateMethod, model);

        /// <summary>
        /// Update the given list of models with the associated repository.
        /// </summary>
        /// <param name="entities">Models to update.</param>
        /// <typeparam name="TKey">Id type of the model.</typeparam>
        /// <returns>Task that completes when the models are updated.</returns>
        public static Task Update<TKey>(this IEnumerable<Base<TKey>> entities) => Execute(
            UpdateMethod,
            entities,
            entities.GetType().GenericTypeArguments.First());

        /// <summary>
        /// Save the given model with the associated repository.
        /// </summary>
        /// <param name="model">Model to save.</param>
        /// <typeparam name="TKey">Id type of the model.</typeparam>
        /// <returns>Task that completes when the model is saved.</returns>
        public static Task Save<TKey>(this Base<TKey> model) => Execute(SaveMethod, model);

        /// <summary>
        /// Save the given list of models with the associated repository.
        /// </summary>
        /// <param name="entities">Models to save.</param>
        /// <typeparam name="TKey">Id type of the model.</typeparam>
        /// <returns>Task that completes when the models are saved.</returns>
        public static Task Save<TKey>(this IEnumerable<Base<TKey>> entities) => Execute(
            SaveMethod,
            entities,
            entities.GetType().GenericTypeArguments.First());

        /// <summary>
        /// Delete the given model with the associated repository.
        /// </summary>
        /// <param name="model">Model to delete.</param>
        /// <typeparam name="TKey">Id type of the model.</typeparam>
        /// <returns>Task that completes when the model is deleted.</returns>
        public static Task Delete<TKey>(this Base<TKey> model) => Execute(DeleteMethod, model);

        /// <summary>
        /// Delete the given list of models with the associated repository.
        /// </summary>
        /// <param name="entities">Models to delete.</param>
        /// <typeparam name="TKey">Id type of the model.</typeparam>
        /// <returns>Task that completes when the models are deleted.</returns>
        public static Task Delete<TKey>(this IEnumerable<Base<TKey>> entities) => Execute(
            DeleteMethod,
            entities,
            entities.GetType().GenericTypeArguments.First());

        /// <summary>
        /// Execute a method on the repository.
        /// </summary>
        /// <param name="methodName">The method to execute.</param>
        /// <param name="param">Param for the method.</param>
        /// <param name="type">Specific repo type if needed. (used for IEnumerable calls)</param>
        /// <returns>A task that resolves when the method was executed.</returns>
        /// <exception cref="ArgumentNullException">When no method was found.</exception>
        private static Task Execute(string methodName, object param, Type type = null)
        {
            var repositoryType = GetRepositoryType(type ?? param.GetType());
            var repository = _provider.GetRequiredService(repositoryType);

            var method = repository.GetType().GetMethod(methodName, new[] { param.GetType() });
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method), "The given method was not found.");
            }

            return method.Invoke(repository, new[] { param }) as Task ?? Task.CompletedTask;
        }

        /// <summary>
        /// Return the repository type for a given model type.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <returns>Type of the repository for that given model.</returns>
        /// <exception cref="ArgumentNullException">
        /// When the <see cref="ServiceProvider"/> is not set (i.e. <see cref="UseModelExtensions"/> was not called.)
        /// </exception>
        /// <exception cref="KeyNotFoundException">The requested type is not present in the registry.</exception>
        private static Type GetRepositoryType(Type modelType)
        {
            if (_provider == null)
            {
                throw new ArgumentNullException(
                    nameof(_provider),
                    "Service provider not registered. Did you forget to call `UseModelExtensions`?");
            }

            return ModelRepositories[modelType];
        }
    }
}
