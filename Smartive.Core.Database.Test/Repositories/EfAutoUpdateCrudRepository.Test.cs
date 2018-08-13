using System.Threading.Tasks;
using FluentAssertions;
using Smartive.Core.Database.Repositories;
using Smartive.Core.Database.Test.Models;
using Xunit;

namespace Smartive.Core.Database.Test.Repositories
{
    public class EfAutoUpdateCrudRepositoryTest
    {
        [Fact]
        public async Task TestInsertModel()
        {
            var context = new TestInMemoryContext();
            var repo = new EfAutoUpdateCrudRepository<AutoUpdateModel>(context.AutoUpdateModels, context);

            var model = new AutoUpdateModel
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
        public async Task TestAutoUpdateModel()
        {
            var context = new TestInMemoryContext();
            var repo = new EfAutoUpdateCrudRepository<AutoUpdateModel>(context.AutoUpdateModels, context);

            var model = new AutoUpdateModel
            {
                Name = "Hans Muster",
                Age = 40
            };

            var entity = await repo.Create(model);

            entity.Age = 50;
            entity.Name = "New Name";

            await repo.Update(entity);

            entity = await repo.GetById(entity.Id);

            entity.Name.Should().Be("New Name");
            entity.Age.Should().Be(50);
        }

        [Fact]
        public async Task TestAutoPropertyUpdateModel()
        {
            var context = new TestInMemoryContext();
            var repo = new EfAutoUpdateCrudRepository<AutoPropertyUpdateModel>(context.AutoPropertyUpdateModels,
                context);

            var model = new AutoPropertyUpdateModel
            {
                Name = "Hans Muster",
                Age = 40
            };

            var entity = await repo.Create(model);
            var id = entity.Id;

            entity = await repo.GetById(id);
            entity.Age = 50;
            entity.Name = "New Name";

            await repo.Update(entity);

            entity = await repo.GetById(entity.Id);

            entity.Name.Should().Be("New Name");
            entity.Age.Should().Be(50);
        }
    }
}
