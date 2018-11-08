using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using Smartive.Core.Debug.Mvc;
using Smartive.Core.Debug.Mvc.Routing;
using Xunit;

namespace Smartive.Core.Debug.Test.Mvc
{
    public class MvcExtensionsTest
    {
        [Fact]
        public void AddMvcRouteAnalyzer_Should_AddTheCorrectDependencies()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddMvcRouteAnalyzer();

            collection.Count.Should().Be(2);

            collection[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
            collection[0].ServiceType.Should().Be(typeof(RouteAnalyzer));

            collection[1].Lifetime.Should().Be(ServiceLifetime.Singleton);
            collection[1].ServiceType.Should().Be(typeof(RazorLightEngine));
        }
    }
}
