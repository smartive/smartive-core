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
        public void TestAddAutoRepositoryAddEntityScoped()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<TestModel, TestInMemoryContext>(
                context => context.Models
            );

            collection.Count.Should().Be(1);
            collection[0].Lifetime.Should().Be(ServiceLifetime.Scoped);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<TestModel>));
        }
        
        [Fact]
        public void TestAddAutoRepositoryAddEntityTransient()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<TestModel, TestInMemoryContext>(
                context => context.Models,
                ServiceLifetime.Transient
            );

            collection.Count.Should().Be(1);
            collection[0].Lifetime.Should().Be(ServiceLifetime.Transient);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<TestModel>));
        }
        
        [Fact]
        public void TestAddAutoRepositoryAddEntitySingleton()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<TestModel, TestInMemoryContext>(
                context => context.Models,
                ServiceLifetime.Singleton
            );

            collection.Count.Should().Be(1);
            collection[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<TestModel>));
        }
        
        [Fact]
        public void TestAddAutoRepositoryAddKeyEntityScoped()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<string, TestModelKey, TestInMemoryContext>(
                context => context.KeyModels
            );

            collection.Count.Should().Be(1);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<string, TestModelKey>));
            collection[0].Lifetime.Should().Be(ServiceLifetime.Scoped);
        }
        
        [Fact]
        public void TestAddAutoRepositoryAddKeyEntityTransient()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<string, TestModelKey, TestInMemoryContext>(
                context => context.KeyModels,
                ServiceLifetime.Transient
            );

            collection.Count.Should().Be(1);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<string, TestModelKey>));
            collection[0].Lifetime.Should().Be(ServiceLifetime.Transient);
        }
        
        [Fact]
        public void TestAddAutoRepositoryAddKeyEntitySingleton()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<string, TestModelKey, TestInMemoryContext>(
                context => context.KeyModels,
                ServiceLifetime.Singleton
            );

            collection.Count.Should().Be(1);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<string, TestModelKey>));
            collection[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
        }
        
        [Fact]
        public void TestAddAutoRepositoryAddByReflectionEntityScoped()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<TestModel, TestInMemoryContext>();

            collection.Count.Should().Be(1);
            collection[0].Lifetime.Should().Be(ServiceLifetime.Scoped);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<TestModel>));
        }
        
        [Fact]
        public void TestAddAutoRepositoryAddByReflectionEntityTransient()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<TestModel, TestInMemoryContext>(
                ServiceLifetime.Transient
            );

            collection.Count.Should().Be(1);
            collection[0].Lifetime.Should().Be(ServiceLifetime.Transient);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<TestModel>));
        }
        
        [Fact]
        public void TestAddAutoRepositoryAddByReflectionEntitySingleton()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<TestModel, TestInMemoryContext>(
                ServiceLifetime.Singleton
            );

            collection.Count.Should().Be(1);
            collection[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<TestModel>));
        }
        
        [Fact]
        public void TestAddAutoRepositoryAddByReflectionKeyEntityScoped()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<string, TestModelKey, TestInMemoryContext>();

            collection.Count.Should().Be(1);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<string, TestModelKey>));
            collection[0].Lifetime.Should().Be(ServiceLifetime.Scoped);
        }
        
        [Fact]
        public void TestAddAutoRepositoryAddByReflectionKeyEntityTransient()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<string, TestModelKey, TestInMemoryContext>(
                ServiceLifetime.Transient
            );

            collection.Count.Should().Be(1);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<string, TestModelKey>));
            collection[0].Lifetime.Should().Be(ServiceLifetime.Transient);
        }
        
        [Fact]
        public void TestAddAutoRepositoryAddByReflectionKeyEntitySingleton()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddAutoRepository<string, TestModelKey, TestInMemoryContext>(
                ServiceLifetime.Singleton
            );

            collection.Count.Should().Be(1);
            collection[0].ServiceType.Should().Be(typeof(ICrudRepository<string, TestModelKey>));
            collection[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void TestAddAutoRepositoryReturnCorrectAutoRepository()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddScoped<TestInMemoryContext>();
            collection.AddAutoRepository<TestModel, TestInMemoryContext>(
                context => context.Models
            );

            var result = collection.BuildServiceProvider().GetService<ICrudRepository<TestModel>>();
            result.GetType().Should().Be(typeof(EfAutoUpdateCrudRepository<TestModel, TestInMemoryContext>));
        }
        
        [Fact]
        public void TestAddAutoRepositoryReturnCorrectKeyAutoRepository()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddScoped<TestInMemoryContext>();
            collection.AddAutoRepository<string, TestModelKey, TestInMemoryContext>(
                context => context.KeyModels
            );

            var result = collection.BuildServiceProvider().GetService<ICrudRepository<string, TestModelKey>>();
            result.GetType().Should().Be(typeof(EfAutoUpdateCrudRepository<string, TestModelKey, TestInMemoryContext>));
        }
        
        [Fact]
        public void TestAddAutoRepositoryReturnCorrectReflectionAutoRepository()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddScoped<TestInMemoryContext>();
            collection.AddAutoRepository<TestModel, TestInMemoryContext>();

            var result = collection.BuildServiceProvider().GetService<ICrudRepository<TestModel>>();
            result.GetType().Should().Be(typeof(EfAutoUpdateCrudRepository<TestModel, TestInMemoryContext>));
        }
        
        [Fact]
        public void TestAddAutoRepositoryReturnCorrectReflectionKeyAutoRepository()
        {
            var collection = new ServiceCollection();
            collection.Count.Should().Be(0);

            collection.AddScoped<TestInMemoryContext>();
            collection.AddAutoRepository<string, TestModelKey, TestInMemoryContext>();

            var result = collection.BuildServiceProvider().GetService<ICrudRepository<string, TestModelKey>>();
            result.GetType().Should().Be(typeof(EfAutoUpdateCrudRepository<string, TestModelKey, TestInMemoryContext>));
        }
    }
}
