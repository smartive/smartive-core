using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Test.Models;

namespace Smartive.Core.Database.Test
{
    public class TestDataContextFixture : IDisposable
    {
        public TestDataContextFixture()
        {
            Context = new TestDataContext();
        }
        
        public TestDataContext Context { get; }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
    
    public class TestDataContext : DbContext
    {
        private readonly string _dbName;

        public TestDataContext()
            : this(Guid.NewGuid().ToString())
        {
        }

        public TestDataContext(string dbName)
            : base(
                new DbContextOptionsBuilder<TestDataContext>()
                    .UseSqlite($"Data Source=./{dbName}.sqlite")
                    .Options)
        {
            _dbName = dbName;
            Setup();
            Clean();
        }

        public DbSet<Author> Authors { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public void Setup()
        {
            Database.Migrate();
        }

        public void Clean()
        {
            Authors.RemoveRange(Authors);
            Books.RemoveRange(Books);
            Comments.RemoveRange(Comments);

            SaveChanges();
        }

        public override void Dispose()
        {
            base.Dispose();
            File.Delete($"./{_dbName}.sqlite");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Author>()
                .HasMany(author => author.Books)
                .WithOne(book => book.Author);

            modelBuilder
                .Entity<Book>()
                .HasMany(book => book.Comments)
                .WithOne(comment => comment.Book);
        }
    }
}
