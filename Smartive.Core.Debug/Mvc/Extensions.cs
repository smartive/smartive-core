using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using Smartive.Core.Debug.Mvc.Routing;

namespace Smartive.Core.Debug.Mvc
{
    /// <summary>
    /// Method extensions for mvc debug features.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Add the MvcRouteAnalyzer to the services. Effectively adds the <see cref="RouteAnalyzer"/>
        /// and a <see cref="RazorLightEngine"/> to the service collection.
        /// </summary>
        /// <param name="services">The service collection that gets the singletons attached.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddMvcRouteAnalyzer(this IServiceCollection services)
        {
            return services
                .AddSingleton<RouteAnalyzer>()
                .AddSingleton(
                    new RazorLightEngineBuilder()
                        .UseEmbeddedResourcesProject(typeof(RouteAnalyzer))
                        .UseMemoryCachingProvider()
                        .Build());
        }

        /// <summary>
        /// Enable the MvcRouteAnalyzer by adding a custom router to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="app">The application that gets the router added.</param>
        /// <param name="path">The url template path of the endpoint that displays all registered routes.</param>
        /// <returns>The given instance of the application builder.</returns>
        public static IApplicationBuilder UseMvcRouteAnalyzer(this IApplicationBuilder app, string path = "/routes")
        {
            return app.UseRouter(
                builder => builder.MapGet(
                    path,
                    builder.ServiceProvider.GetService<RouteAnalyzer>().Handler));
        }
    }
}
