using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Smartive.Core.Database.Extensions;
using Smartive.Core.Database.Repositories;
using Smartive.Core.Database.Test.Models;
using Xunit;

namespace Smartive.Core.Database.Test.Extensions
{
    public class ServiceCollectionExtensionsTest
    {
        [Fact]
        public void Test_AddRepository_AddEntity_Scoped()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddRepository<Author, TestDataContext>(ServiceLifetime.Scoped);

            collection.Count.Should().Be(1);
            collection[0].Lifetime.Should().Be(ServiceLifetime.Scoped);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<Author>));
        }

        [Fact]
        public void Test_AddRepository_AddEntity_Transient()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddRepository<Author, TestDataContext>();

            collection.Count.Should().Be(1);
            collection[0].Lifetime.Should().Be(ServiceLifetime.Transient);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<Author>));
        }

        [Fact]
        public void Test_AddRepository_AddEntity_Singleton()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddRepository<Author, TestDataContext>(ServiceLifetime.Singleton);

            collection.Count.Should().Be(1);
            collection[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<Author>));
        }

        [Fact]
        public void Test_AddRepository_Return_Correct_AutoRepository()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddScoped<TestDataContext>();
            collection.AddRepository<Author, TestDataContext>();

            var result = collection.BuildServiceProvider().GetService<ICrudRepository<Author>>();
            result.GetType().Should().Be(typeof(EfCrudRepository<Author, TestDataContext>));
        }
    }
}
