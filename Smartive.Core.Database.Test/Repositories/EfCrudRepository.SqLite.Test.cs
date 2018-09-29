using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Repositories;
using Smartive.Core.Database.Test.Models;
using Xunit;

namespace Smartive.Core.Database.Test.Repositories
{
    public class EfCrudRepositorySqLiteTest : IDisposable
    {
        private class EfCrudRepositoryInstance : EfCrudRepository<TestModel>
        {
            public EfCrudRepositoryInstance(DbSet<TestModel> entities, DbContext context) : base(entities, context)
            {
            }

            protected override void UpdateEntity(ref TestModel dbEntity, TestModel entity)
            {
                dbEntity.Name = entity.Name;
                dbEntity.Age = entity.Age;
            }
        }

        private readonly TestSqLiteContext _context;
        private readonly EfCrudRepositoryInstance _repository;

        public EfCrudRepositorySqLiteTest()
        {
            _context = new TestSqLiteContext(Guid.NewGuid().ToString());
            _context.CreateTables();
            _repository = new EfCrudRepositoryInstance(_context.Models, _context);
        }
        
        public void Dispose()
        {
            _context?.Dispose();
        }

        [Fact]
        public async Task TestGetEmptyList()
        {
            var result = await _repository.GetAll();
            result.Count().Should().Be(0);
        }
        
        [Fact]
        public async Task TestInsertModel()
        {
            var model = new TestModel
            {
                Name = "Hans Muster",
                Age = 40
            };

            model.Name.Should().Be("Hans Muster");
            model.Id.Should().Be(0);

            await _repository.Create(model);

            model.Id.Should().Be(1);
        }
        
        [Fact]
        public async Task TestUpdateModel()
        {
            var model = new TestModel
            {
                Name = "Hans Muster",
                Age = 40
            };

            var entity = await _repository.Create(model);

            entity.Name = "Hans Meier";

            await _repository.Update(entity);

            entity = await _repository.GetById(entity.Id);

            entity.Name.Should().Be("Hans Meier");
        }
        
        [Fact]
        public async Task TestGetModel()
        {
            var model = new TestModel
            {
                Name = "Peterhans",
                Age = 42
            };

            var entity = await _repository.Create(model);

            model.Id.Should().NotBe(0);

            var found = await _repository.GetById(entity.Id);

            found.Should().NotBe(null);
        }
        
        [Fact]
        public async Task TestDeleteModel()
        {
            var model = new TestModel
            {
                Name = "Peterhans",
                Age = 42
            };

            var entity = await _repository.Create(model);

            model.Id.Should().NotBe(0);

            var found = await _repository.GetById(entity.Id);

            found.Should().NotBe(null);

            await _repository.DeleteById(entity.Id);

            found = await _repository.GetById(entity.Id);

            found.Should().Be(null);
        }
        
        [Fact]
        public async Task TestGetAllModel()
        {
            await _repository.Create(new TestModel
            {
                Name = "Peterhans",
                Age = 42
            });
            await _repository.Create(new TestModel
            {
                Name = "Hanspeter",
                Age = 42
            });

            var found = await _repository.GetAll();

            found.Count().Should().Be(2);
        }
    }
}
