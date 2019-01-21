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
        private readonly TestDataContext _context;

        private readonly ICrudRepository<Author> _authors;
        private readonly ICrudRepository<Book> _books;
        private readonly ICrudRepository<Comment> _comments;

        public EfCrudRepositoryTest()
        {
            _context = new TestDataContext();
            _authors = new EfCrudRepository<Author>(_context);
            _books = new EfCrudRepository<Book>(_context);
            _comments = new EfCrudRepository<Comment>(_context);
        }

        [Fact]
        public async Task Test_Create_Single_Author()
        {
            var author = new Author
            {
                Name = "Author 1"
            };

            author.Id.Should().Be(default);
            author = await _authors.Create(author);
            author.Id.Should().NotBe(default);
        }

        [Fact]
        public async Task Test_Create_Single_Book()
        {
            await _authors.Create(
                new Author
                {
                    Name = "Author 1"
                });

            var book = new Book
            {
                Name = "Book 1",
                AuthorId = 1
            };

            book.Id.Should().Be(default);
            book = await _books.Create(book);
            book.Id.Should().NotBe(default);
        }

        [Fact]
        public async Task Test_Create_Single_Comment()
        {
            await _authors.Create(
                new Author
                {
                    Name = "Author 1"
                });

            await _books.Create(
                new Book
                {
                    Name = "Book 1",
                    AuthorId = 1
                });

            var comment = new Comment
            {
                Message = "Comment 1",
                BookId = 1
            };

            comment.Id.Should().Be(default);
            comment = await _comments.Create(comment);
            comment.Id.Should().NotBe(default);
        }

        [Fact]
        public async Task Test_Create_Multiple_Author()
        {
            var authors = await _authors.Create(
                new[] { new Author { Name = "A1" }, new Author { Name = "A2" }, new Author { Name = "A3" } });

            authors.All(a => a.Id != default).Should().BeTrue();
        }

        [Fact]
        public async Task Test_Create_Multiple_Books()
        {
            await _authors.Create(
                new[] { new Author { Name = "A1" }, new Author { Name = "A2" }, new Author { Name = "A3" } });

            var books = await _books.Create(
                new[]
                {
                    new Book { Name = "B1", AuthorId = 1 },
                    new Book { Name = "B2", AuthorId = 2 },
                    new Book { Name = "B3", AuthorId = 2 }
                });

            books.All(a => a.Id != default).Should().BeTrue();
        }

        [Fact]
        public async Task Test_Create_Multiple_Comments()
        {
            await _authors.Create(
                new[]
                {
                    new Author { Name = "A1" },
                    new Author { Name = "A2" },
                    new Author { Name = "A3" }
                });

            await _books.Create(
                new[]
                {
                    new Book { Name = "B1", AuthorId = 1 },
                    new Book { Name = "B2", AuthorId = 2 },
                    new Book { Name = "B3", AuthorId = 2 }
                });

            var comments = await _comments.Create(
                new[]
                {
                    new Comment { Message = "Message 1", BookId = 1 },
                    new Comment { Message = "Message 2", BookId = 1 },
                    new Comment { Message = "Message 3", BookId = 1 }
                });

            comments.All(a => a.Id != default).Should().BeTrue();
        }

        [Fact]
        public async Task Test_Update_Single_Author()
        {
            await InsertDemoData();

            var updated = new Author
            {
                Id = 1,
                Name = "New Updated Author.",
                Age = 49
            };

            await _authors.Update(updated);

            var result = await _context.Authors.SingleAsync(a => a.Id == 1);
            result.Name.Should().Be("New Updated Author.");
            result.Age.Should().Be(49);
        }

        [Fact]
        public async Task Test_Update_Single_Book()
        {
            await InsertDemoData();

            var updated = new Book
            {
                Id = 1,
                Name = "New Updated Book.",
                AuthorId = 3
            };

            await _books.Update(updated);

            var result = await _context.Books
                .Include(o => o.Author)
                .SingleAsync(o => o.Id == 1);
            result.Name.Should().Be("New Updated Book.");
            result.Author.Name.Should().Be("A3");
        }

        [Fact]
        public async Task Test_Update_Single_Comment()
        {
            await InsertDemoData();

            var updated = new Comment
            {
                Id = 1,
                Message = "New Updated Comment.",
                BookId = 2
            };

            await _comments.Update(updated);

            var result = await _context.Comments
                .Include(o => o.Book)
                .SingleAsync(o => o.Id == 1);
            result.Message.Should().Be("New Updated Comment.");
            result.Book.Name.Should().Be("B2");
        }

        [Fact]
        public async Task Test_Update_Single_Detached_Author()
        {
            await InsertDemoData();

            foreach (var entry in _context.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }

            var updated = new Author
            {
                Id = 1,
                Name = "New Updated Author.",
                Age = 49
            };

            await _authors.Update(updated);

            var result = await _context.Authors.SingleAsync(a => a.Id == 1);
            result.Name.Should().Be("New Updated Author.");
            result.Age.Should().Be(49);
        }

        [Fact]
        public async Task Test_Update_Single_Detached_Book()
        {
            await InsertDemoData();

            foreach (var entry in _context.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }

            var updated = new Book
            {
                Id = 1,
                Name = "New Updated Book.",
                AuthorId = 3
            };

            await _books.Update(updated);

            var result = await _context.Books
                .Include(o => o.Author)
                .SingleAsync(o => o.Id == 1);
            result.Name.Should().Be("New Updated Book.");
            result.Author.Name.Should().Be("A3");
        }

        [Fact]
        public async Task Test_Update_Single_Detached_Comment()
        {
            await InsertDemoData();

            foreach (var entry in _context.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }

            var updated = new Comment
            {
                Id = 1,
                Message = "New Updated Comment.",
                BookId = 2
            };

            await _comments.Update(updated);

            var result = await _context.Comments
                .Include(o => o.Book)
                .SingleAsync(o => o.Id == 1);
            result.Message.Should().Be("New Updated Comment.");
            result.Book.Name.Should().Be("B2");
        }

        [Fact]
        public async Task Test_Update_Multiple_Author()
        {
            await InsertDemoData();

            await _authors.Update(
                new[]
                {
                    new Author
                    {
                        Id = 1,
                        Name = "New Updated Author.",
                        Age = 49
                    },
                    new Author
                    {
                        Id = 2,
                        Name = "The Author",
                        Age = 42
                    }
                });

            var result = await _context.Authors.ToListAsync();
            result.First(o => o.Id == 1).Name.Should().Be("New Updated Author.");
            result.First(o => o.Id == 1).Age.Should().Be(49);
            result.First(o => o.Id == 2).Name.Should().Be("The Author");
            result.First(o => o.Id == 2).Age.Should().Be(42);
        }

        [Fact]
        public async Task Test_Update_Multiple_Book()
        {
            await InsertDemoData();

            await _books.Update(
                new[]
                {
                    new Book
                    {
                        Id = 1,
                        Name = "New Updated Book.",
                        AuthorId = 3
                    },
                    new Book
                    {
                        Id = 2,
                        Name = "New Updated Book.",
                        AuthorId = 3
                    }
                });

            var result = await _context.Books.ToListAsync();
            result.First(o => o.Id == 1).Name.Should().Be("New Updated Book.");
            result.First(o => o.Id == 1).AuthorId.Should().Be(3);
            result.First(o => o.Id == 2).Name.Should().Be("New Updated Book.");
            result.First(o => o.Id == 2).AuthorId.Should().Be(3);
        }

        [Fact]
        public async Task Test_Update_Multiple_Detached_Author()
        {
            await InsertDemoData();

            foreach (var entry in _context.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }

            await _authors.Update(
                new[]
                {
                    new Author
                    {
                        Id = 1,
                        Name = "New Updated Author.",
                        Age = 49
                    },
                    new Author
                    {
                        Id = 2,
                        Name = "The Author",
                        Age = 42
                    }
                });

            var result = await _context.Authors.ToListAsync();
            result.First(o => o.Id == 1).Name.Should().Be("New Updated Author.");
            result.First(o => o.Id == 1).Age.Should().Be(49);
            result.First(o => o.Id == 2).Name.Should().Be("The Author");
            result.First(o => o.Id == 2).Age.Should().Be(42);
        }

        [Fact]
        public async Task Test_Update_Multiple_Detached_Book()
        {
            await InsertDemoData();

            foreach (var entry in _context.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }

            await _books.Update(
                new[]
                {
                    new Book
                    {
                        Id = 1,
                        Name = "New Updated Book.",
                        AuthorId = 3
                    },
                    new Book
                    {
                        Id = 2,
                        Name = "New Updated Book.",
                        AuthorId = 3
                    }
                });

            var result = await _context.Books.ToListAsync();
            result.First(o => o.Id == 1).Name.Should().Be("New Updated Book.");
            result.First(o => o.Id == 1).AuthorId.Should().Be(3);
            result.First(o => o.Id == 2).Name.Should().Be("New Updated Book.");
            result.First(o => o.Id == 2).AuthorId.Should().Be(3);
        }

        [Fact]
        public async Task Test_Save_Single_Unsaved_Author()
        {
            await InsertDemoData();
            await _authors.Save(new Author { Name = "A4" });
            var result = await _context.Authors.FirstAsync(o => o.Id == 4);
            result.Id.Should().Be(4);
            result.Name.Should().Be("A4");
        }

        [Fact]
        public async Task Test_Save_Single_Saved_Author()
        {
            await InsertDemoData();
            await _authors.Save(new Author { Id = 3, Name = "Updated" });
            var result = await _context.Authors.FirstAsync(o => o.Id == 3);
            result.Id.Should().Be(3);
            result.Name.Should().Be("Updated");
        }

        [Fact]
        public async Task Test_Save_Multiple_Unsaved_Author()
        {
            await InsertDemoData();
            await _authors.Save(new[] { new Author { Name = "A4" }, new Author { Name = "A5" } });
            var result = await _context.Authors.ToListAsync();
            result.First(o => o.Id == 4).Name.Should().Be("A4");
            result.First(o => o.Id == 5).Name.Should().Be("A5");
        }

        [Fact]
        public async Task Test_Save_Multiple_Saved_Author()
        {
            await InsertDemoData();
            await _authors.Save(
                new[] { new Author { Id = 2, Name = "AUTHOR_2" }, new Author { Id = 3, Name = "AUTHOR_3" } });
            var result = await _context.Authors.ToListAsync();
            result.First(o => o.Id == 2).Name.Should().Be("AUTHOR_2");
            result.First(o => o.Id == 3).Name.Should().Be("AUTHOR_3");
        }

        [Fact]
        public async Task Test_Save_Multiple_Mixed_Author()
        {
            await InsertDemoData();
            await _authors.Save(
                new[]
                {
                    new Author { Id = 2, Name = "AUTHOR_2" },
                    new Author
                    {
                        Name = "NEW_AUTHOR"
                    }
                });
            var result = await _context.Authors.ToListAsync();
            result.First(o => o.Id == 2).Name.Should().Be("AUTHOR_2");
            result.First(o => o.Id == 4).Name.Should().Be("NEW_AUTHOR");
        }

        [Fact]
        public async Task Test_AsQueryable()
        {
            await InsertDemoData();

            var result = await _authors
                .AsQueryable()
                .Include(o => o.Books)
                .Where(o => o.Id >= 2)
                .ToListAsync();

            result[0].Name.Should().Be("A2");
            result[0].Books.Should().NotBeNull();
            result[1].Name.Should().Be("A3");
            result[1].Books.Should().NotBeNull();
        }

        [Fact]
        public async Task Test_GetAll()
        {
            await InsertDemoData();

            var result = await _authors.GetAll();

            result.Count().Should().Be(3);
        }

        [Fact]
        public async Task Test_GetById()
        {
            await InsertDemoData();

            var result = await _authors.GetById(1);
            result.Name.Should().Be("A1");
        }

        [Fact]
        public async Task Test_DeleteById()
        {
            await InsertDemoData();

            await _authors.DeleteById(1);

            var result = await _authors.GetAll();
            result.Count().Should().Be(2);
        }

        [Fact]
        public async Task Test_Delete()
        {
            await InsertDemoData();

            await _authors.Delete(new Author { Id = 1 });

            var result = await _authors.GetAll();
            result.Count().Should().Be(2);
        }

        [Fact]
        public async Task Test_Delete_Multiple_ById()
        {
            await InsertDemoData();

            await _authors.DeleteById(new[] { 1, 2, 3 });

            var result = await _authors.GetAll();
            result.Count().Should().Be(0);
        }

        [Fact]
        public async Task Test_Delete_Multiple()
        {
            await InsertDemoData();

            await _authors.Delete(new[] { new Author { Id = 1 }, new Author { Id = 2 }, new Author { Id = 3 } });

            var result = await _authors.GetAll();
            result.Count().Should().Be(0);
        }

        private async Task InsertDemoData()
        {
            await _authors.Create(
                new[]
                {
                    new Author { Name = "A1" },
                    new Author { Name = "A2" },
                    new Author { Name = "A3" }
                });

            await _books.Create(
                new[]
                {
                    new Book { Name = "B1", AuthorId = 1 },
                    new Book { Name = "B2", AuthorId = 2 },
                    new Book { Name = "B3", AuthorId = 2 }
                });

            await _comments.Create(
                new[]
                {
                    new Comment { Message = "Message 1", BookId = 1 },
                    new Comment { Message = "Message 2", BookId = 1 },
                    new Comment { Message = "Message 3", BookId = 1 }
                });
        }
    }
}
