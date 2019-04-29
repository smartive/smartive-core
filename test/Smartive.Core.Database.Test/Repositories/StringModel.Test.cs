using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Repositories;
using Smartive.Core.Database.Test.Models;
using Xunit;

namespace Smartive.Core.Database.Test.Repositories
{
    public class StringModelTest
    {
        private readonly TestDataContext _context;

        private readonly ICrudRepository<string, User> _users;
        private readonly ICrudRepository<UserItem> _userItems;

        public StringModelTest()
        {
            _context = TestDataContext.PreparedDb();
            _users = new EfCrudRepository<string, User, TestDataContext>(_context);
            _userItems = new EfCrudRepository<UserItem>(_context);
        }

        [Fact]
        public async Task Test_Insert_User()
        {
            var user = new User
            {
                Id = "id",
                Name = "name"
            };

            var dbUser = await _users.GetById("id");
            dbUser.Should().BeNull();
            await _users.Create(user);
            dbUser = await _users.GetById("id");
            dbUser.Should().NotBeNull();
        }

        [Fact]
        public async Task Test_Save_Insert_User()
        {
            var user = new User
            {
                Id = "foobar-id",
                Name = "name"
            };

            var dbUser = await _users.GetById("foobar-id");
            dbUser.Should().BeNull();
            await _users.Save(user);
            dbUser = await _users.GetById("foobar-id");
            dbUser.Should().NotBeNull();
        }

        [Fact]
        public async Task Test_Save_Update_User()
        {
            await InsertDemoData();

            var user = new User
            {
                Id = "user2",
                Name = "name"
            };

            await _users.Save(user);

            user = await _users.GetById("user2");
            user.Name.Should().Be("name");
        }

        [Fact]
        public async Task Test_Update_User()
        {
            await InsertDemoData();
            DetachData();

            var user = await _users.GetById("user1");
            user.Name.Should().Be("User1");
            user.UserItems.Should().BeNull();

            user.Name = "NewUser1";
            await _users.Update(user);

            user = await _users.GetById("user1");
            user.Name.Should().Be("NewUser1");
        }

        [Fact]
        public async Task Test_Get_User_With_Items()
        {
            await InsertDemoData();
            DetachData();

            var user = await _users.QuerySingle(users => users.Where(u => u.Id == "user1").Include(u => u.UserItems));
            user.Name.Should().Be("User1");
            user.UserItems.Should().NotBeNull();
        }

        [Fact]
        public async Task Test_Update_User_With_Items_List()
        {
            await InsertDemoData();
            DetachData();

            var user = await _users.QuerySingle(users => users.Where(u => u.Id == "user1").Include(u => u.UserItems));
            user.Name.Should().Be("User1");
            user.UserItems.Should().NotBeNull();

            user.Name = "NewUserName";

            await _users.Save(user);

            DetachData();

            user = await _users.QuerySingle(users => users.Where(u => u.Id == "user1").Include(u => u.UserItems));
            user.Name.Should().Be("NewUserName");
            user.UserItems.Should().NotBeNull();
        }

        [Fact]
        public async Task Test_Update_Included_Data()
        {
            await InsertDemoData();
            DetachData();

            var user = await _users.QuerySingle(users => users.Where(u => u.Id == "user1").Include(u => u.UserItems));
            user.Name.Should().Be("User1");
            user.UserItems[0].Name.Should().Be("Useritem 1");

            user.UserItems[0].Name = "YOLO";

            await _users.Save(user);

            DetachData();

            user = await _users.QuerySingle(users => users.Where(u => u.Id == "user1").Include(u => u.UserItems));
            user.Name.Should().Be("User1");
            user.UserItems[0].Name.Should().Be("YOLO");
        }

        private async Task InsertDemoData()
        {
            await _users.Create(
                new[]
                {
                    new User { Id = "user1", Name = "User1" },
                    new User { Id = "user2", Name = "User2" }
                });

            await _userItems.Create(
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
    }
}
