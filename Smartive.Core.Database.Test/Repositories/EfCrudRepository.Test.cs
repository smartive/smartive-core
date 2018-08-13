using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Repositories;
using Smartive.Core.Database.Test.Models;
using Xunit;

namespace Smartive.Core.Database.Test.Repositories
{
    public class EfCrudRepositoryTest
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

        [Fact]
        public async Task TestInsertModel()
        {
            var context = new TestInMemoryContext();
            var repo = new EfCrudRepositoryInstance(context.Models, context);

            var model = new TestModel
            {
                Name = "Hans Muster",
                Age = 40
            };

            model.Name.Should().Be("Hans Muster");
            model.Id.Should().Be(0);

            await repo.Create(model);

            model.Id.Should().NotBe(0);
        }

        [Fact]
        public async Task TestUpdateModel()
        {
            var context = new TestInMemoryContext();
            var repo = new EfCrudRepositoryInstance(context.Models, context);

            var model = new TestModel
            {
                Name = "Hans Muster",
                Age = 40
            };

            var entity = await repo.Create(model);

            entity.Name = "Hans Meier";

            await repo.Update(entity);

            entity = await repo.GetById(entity.Id);

            entity.Name.Should().Be("Hans Meier");
        }

        [Fact]
        public async Task TestGetModel()
        {
            var context = new TestInMemoryContext();
            var repo = new EfCrudRepositoryInstance(context.Models, context);

            var model = new TestModel
            {
                Name = "Peterhans",
                Age = 42
            };

            var entity = await repo.Create(model);

            model.Id.Should().NotBe(0);

            var found = await repo.GetById(entity.Id);

            found.Should().NotBe(null);
        }

        [Fact]
        public async Task TestDeleteModel()
        {
            var context = new TestInMemoryContext();
            var repo = new EfCrudRepositoryInstance(context.Models, context);

            var model = new TestModel
            {
                Name = "Peterhans",
                Age = 42
            };

            var entity = await repo.Create(model);

            model.Id.Should().NotBe(0);

            var found = await repo.GetById(entity.Id);

            found.Should().NotBe(null);

            await repo.DeleteById(entity.Id);

            found = await repo.GetById(entity.Id);

            found.Should().Be(null);
        }

        [Fact]
        public async Task TestGetAllModel()
        {
            var context = new TestInMemoryContext("get-all");
            var repo = new EfCrudRepositoryInstance(context.Models, context);

            await repo.Create(new TestModel
            {
                Name = "Peterhans",
                Age = 42
            });
            await repo.Create(new TestModel
            {
                Name = "Hanspeter",
                Age = 42
            });

            var found = await repo.GetAll();

            found.Count().Should().Be(2);
        }

        [Fact]
        public async Task TestAsQueryable()
        {
            var context = new TestInMemoryContext("as-queryable");
            var repo = new EfCrudRepositoryInstance(context.Models, context);

            await repo.Create(new TestModel
            {
                Name = "Peterhans",
                Age = 42
            });
            await repo.Create(new TestModel
            {
                Name = "Hanspeter",
                Age = 42
            });

            var found = repo.AsQueryable();

            found.GetType().Should().BeAssignableTo<IQueryable<TestModel>>();
        }
    }
}
