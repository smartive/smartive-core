using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Smartive.Core.Debug.Mvc;
using Xunit;

namespace Smartive.Core.Debug.Test.Mvc.Routing
{
    public class TestMvcRoutingFactory : WebApplicationFactory<TestMvcRoutingStartup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost
                .CreateDefaultBuilder()
                .UseStartup<TestMvcRoutingStartup>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(".");
            base.ConfigureWebHost(builder);
        }
    }

    public class TestMvcRoutingStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddRouting();
            services.AddLogging();
            services.AddMvcRouteAnalyzer();
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
            app.UseMvcRouteAnalyzer();
        }
    }

    public class RouteAnalyzerTest : IClassFixture<TestMvcRoutingFactory>
    {
        private readonly TestMvcRoutingFactory _factory;

        public RouteAnalyzerTest(TestMvcRoutingFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Should_ReturnRoutePage()
        {
            var client = _factory.CreateClient();

            var response = await client.GetStringAsync("/routes");

            response.Should().Contain("<h3>Registered MVC Routes</h3>");
            response.Should().NotContain("<div class=\"card-header\">");
        }
    }
}
