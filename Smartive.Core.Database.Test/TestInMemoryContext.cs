using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Test.Models;

namespace Smartive.Core.Database.Test
{
    public class TestInMemoryContext : DbContext
    {
        public DbSet<TestModel> Models { get; set; }
        public DbSet<AutoUpdateModel> AutoUpdateModels { get; set; }
        public DbSet<AutoPropertyUpdateModel> AutoPropertyUpdateModels { get; set; }

        public TestInMemoryContext(string name = "in-memory") : base(
            new DbContextOptionsBuilder<TestInMemoryContext>()
                .UseInMemoryDatabase(name)
                .Options
        )
        {
        }
    }
}
