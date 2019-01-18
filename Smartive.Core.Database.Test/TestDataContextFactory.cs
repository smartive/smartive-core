using Microsoft.EntityFrameworkCore.Design;

namespace Smartive.Core.Database.Test
{
    public class TestDataContextFactory : IDesignTimeDbContextFactory<TestDataContext>
    {
        public TestDataContext CreateDbContext(string[] _)
        {
            return new TestDataContext();
        }
    }
}
