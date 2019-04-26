using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using RazorLight;

namespace Smartive.Core.Debug.Mvc.Routing
{
    /// <summary>
    /// RouteAnalyzer class that handles the given route to return all registered action handlers.
    /// </summary>
    public class RouteAnalyzer
    {
        private const string ContentType = "text/html";
        private readonly IActionDescriptorCollectionProvider _provider;
        private readonly RazorLightEngine _viewEngine;

        /// <summary>
        /// Create an instance of the RouteAnalyzer.
        /// </summary>
        /// <param name="provider">The provider that contains all action descriptors.</param>
        /// <param name="viewEngine">A razor light engine to render the view.</param>
        public RouteAnalyzer(IActionDescriptorCollectionProvider provider, RazorLightEngine viewEngine)
        {
            _provider = provider;
            _viewEngine = viewEngine;
        }

        /// <summary>
        /// Handler that is registered within the <see cref="IRouter"/> that returns
        /// all registered routes in a nice view.
        /// </summary>
        /// <param name="request">HttpRequest.</param>
        /// <param name="response">HttpResponse.</param>
        /// <param name="data">RouteData (unused).</param>
        /// <returns>A task that is resolved when all routes are collected and rendered into html.</returns>
        public async Task Handler(HttpRequest request, HttpResponse response, RouteData data)
        {
            response.ContentType = ContentType;
            await response.WriteAsync(
                await _viewEngine.CompileRenderAsync(
                    "Routes",
                    _provider.ActionDescriptors.Items
                        .Select(descriptor => new RouteInformation(descriptor))
                        .OrderBy(info => info.Template)
                        .ToImmutableList()));
        }
    }
}
