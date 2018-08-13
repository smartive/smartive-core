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
    }
}
