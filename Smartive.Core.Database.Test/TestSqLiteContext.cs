using System.IO;
using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Test.Models;

namespace Smartive.Core.Database.Test
{
    public class TestSqLiteContext : DbContext
    {
        private readonly string _name;
        public DbSet<TestModel> Models { get; set; }
        public DbSet<TestModelKey> KeyModels { get; set; }
        public DbSet<AutoUpdateModel> AutoUpdateModels { get; set; }
        public DbSet<AutoPropertyUpdateModel> AutoPropertyUpdateModels { get; set; }

        public TestSqLiteContext(string dbName = "test") : base(
            new DbContextOptionsBuilder<TestSqLiteContext>()
                .UseSqlite($"Data Source={dbName}.db")
                .Options
        )
        {
            _name = $"{dbName}.db";
        }

        public void CreateTables()
        {
            Database.ExecuteSqlCommand(
                $"CREATE TABLE `Models` ( `Name` TEXT, `Age` INTEGER, `Id` INTEGER PRIMARY KEY AUTOINCREMENT )"
            );

            Database.ExecuteSqlCommand(
                $"CREATE TABLE `KeyModels` ( `Name` TEXT, `Age` INTEGER, `Id` TEXT PRIMARY KEY )"
            );

            Database.ExecuteSqlCommand(
                $"CREATE TABLE `AutoUpdateModels` ( `Name` TEXT, `Age` INTEGER, `Id` INTEGER PRIMARY KEY AUTOINCREMENT )"
            );

            Database.ExecuteSqlCommand(
                $"CREATE TABLE `AutoPropertyUpdateModels` ( `Name` TEXT, `Age` INTEGER, `Id` INTEGER PRIMARY KEY AUTOINCREMENT )"
            );

            SaveChanges();
        }

        public override void Dispose()
        {
            base.Dispose();
            File.Delete(_name);
        }
    }
}
