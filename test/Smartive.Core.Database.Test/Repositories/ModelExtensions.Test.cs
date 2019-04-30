using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Smartive.Core.Database.Extensions;
using Smartive.Core.Database.Repositories;
using Smartive.Core.Database.Test.Models;
using Xunit;
using ModelExtensions = Smartive.Core.Database.Extensions.ModelExtensions;

namespace Smartive.Core.Database.Test.Repositories
{
    public class ModelExtensionsTest : IDisposable
    {
        private readonly TestDataContext _context = TestDataContext.PreparedDb();

        [Fact]
        public void Test_Unregistered_Provider_Throws()
        {
            Func<Task> action = async () => { await new User().Save(); };
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Test_Unregistered_Repository_Throws()
        {
            CreateProvider();
            Func<Task> action = async () => { await new Book().Save(); };
            action.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public async Task Test_Create_User()
        {
            await InsertDemoData();
            var o = new User { Id = "LOL", Name = "My Name is SlimShady" };

            await o.Create();

            (await _context.Users.SingleOrDefaultAsync(u => u.Id == "LOL")).Should().NotBeNull();
        }

        [Fact]
        public async Task Test_Versatile_Functions()
        {
            await InsertDemoData();
            var o1 = new User { Id = "LOL", Name = "My Name is SlimShady" };
            var o2 = new UserItem { Name = "MaItem", UserId = "LOL"};

            await o1.Save();
            await o2.Save();

            (await _context.Users.SingleOrDefaultAsync(u => u.Id == "LOL")).Should().NotBeNull();
            (await _context.UserItems.SingleOrDefaultAsync(u => u.Name == "MaItem")).Should().NotBeNull();
        }

        [Fact]
        public async Task Test_Update_User()
        {
            await InsertDemoData();
            var o = new User { Id = "user1", Name = "My Name is SlimShady" };

            await o.Update();

            (await _context.Users.SingleOrDefaultAsync(u => u.Id == "user1")).Name.Should().Be("My Name is SlimShady");
        }

        [Fact]
        public async Task Test_Delete_User()
        {
            await InsertDemoData();
            var o = new User { Id = "user1", Name = "My Name is SlimShady" };

            await o.Delete();

            (await _context.Users.SingleOrDefaultAsync(u => u.Id == "user1")).Should().BeNull();
        }

        [Fact]
        public async Task Test_Save_User()
        {
            await InsertDemoData();
            var o1 = new User { Id = "user1", Name = "My Name is SlimShady" };
            var o2 = new User { Id = "noi", Name = "new" };

            await o1.Save();
            await o2.Save();

            (await _context.Users.SingleOrDefaultAsync(u => u.Id == "user1")).Should().NotBeNull();
            (await _context.Users.SingleOrDefaultAsync(u => u.Id == "noi")).Should().NotBeNull();
        }

        [Fact]
        public async Task Test_Create_User_List()
        {
            await InsertDemoData();
            var o = new List<User>
            {
                new User { Id = "lol1", Name = "lol1" },
                new User { Id = "lol2", Name = "lol2" },
                new User { Id = "lol3", Name = "lol3" }
            };

            await o.Create();

            (await _context.Users.CountAsync()).Should().Be(5);
        }

        [Fact]
        public async Task Test_Update_User_List()
        {
            await InsertDemoData();
            var o = new List<User>
            {
                new User { Id = "user1", Name = "lol1" },
                new User { Id = "user2", Name = "lol2" }
            };

            await o.Update();

            (await _context.Users.CountAsync()).Should().Be(2);
            (await _context.Users.SingleOrDefaultAsync(u => u.Id == "user1")).Name.Should().Be("lol1");
        }

        [Fact]
        public async Task Test_Delete_User_List()
        {
            await InsertDemoData();
            var o = new List<User>
            {
                new User { Id = "user1", Name = "lol1" },
                new User { Id = "user2", Name = "lol2" }
            };

            await o.Delete();

            (await _context.Users.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Test_Save_User_List()
        {
            await InsertDemoData();
            var o = new List<User>
            {
                new User { Id = "lol1", Name = "lol1" },
                new User { Id = "lol2", Name = "lol2" },
                new User { Id = "user1", Name = "lol3" }
            };

            await o.Save();

            (await _context.Users.CountAsync()).Should().Be(4);
        }

        private async Task InsertDemoData()
        {
            var provider = CreateProvider();

            var users = provider.GetService<ICrudRepository<string, User>>();
            var userItems = provider.GetService<ICrudRepository<UserItem>>();

            await users.Create(
                new[]
                {
                    new User { Id = "user1", Name = "User1" },
                    new User { Id = "user2", Name = "User2" }
                });

            await userItems.Create(
                new[]
                {
                    new UserItem { Name = "Useritem 1", UserId = "user1" },
                    new UserItem { Name = "Useritem 2", UserId = "user1" },
                    new UserItem { Name = "Useritem 3", UserId = "user1" },
                    new UserItem { Name = "Useritem 4", UserId = "user2" }
                });
        }

        private void DetachData()
        {
            foreach (var entry in _context.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }

            _context.ChangeTracker.AcceptAllChanges();
        }

        private IServiceProvider CreateProvider()
        {
            var services = new ServiceCollection();

            services
                .AddTransient(_ => _context)
                .AddRepository<string, User, TestDataContext>()
                .AddRepository<UserItem, TestDataContext>();

            var provider = services.BuildServiceProvider();
            provider.UseModelExtensions();

            return provider;
        }

        public void Dispose()
        {
            _context?.Dispose();
            ModelExtensions.ResetRegistrations();
        }
    }
}
