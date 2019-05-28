using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Smartive.Core.Database.Repositories;
using Smartive.Core.Database.Test.Models;
using Xunit;

namespace Smartive.Core.Database.Test.Repositories
{
    public class IgnoreOnUpdateTest
    {
        private readonly TestDataContext _context;

        private readonly ICrudRepository<string, User> _users;

        public IgnoreOnUpdateTest()
        {
            _context = TestDataContext.PreparedDb();
            _users = new EfCrudRepository<string, User, TestDataContext>(_context);
        }

        [Fact]
        public async Task Test_Insert_User()
        {
            var user = new User
            {
                Id = "user",
                Name = "User",
                SetOnUpdate = "This is set on update",
                DontSetOnUpdate = "This is only set on insert"
            };

            user = await _users.Create(user);
            user.SetOnUpdate.Should().Be("This is set on update");
            user.DontSetOnUpdate.Should().Be("This is only set on insert");
        }

        [Fact]
        public async Task Test_Update_User()
        {
            var user = new User
            {
                Id = "user",
                Name = "User",
                SetOnUpdate = "This is set on update",
                DontSetOnUpdate = "This is only set on insert"
            };

            user = await _users.Create(user);
            user.SetOnUpdate.Should().Be("This is set on update");
            user.DontSetOnUpdate.Should().Be("This is only set on insert");

            user.SetOnUpdate = "This should be another value now";
            user.DontSetOnUpdate = "This should not change";

            user = await _users.Update(user);
            user.SetOnUpdate.Should().Be("This should be another value now");
            user.DontSetOnUpdate.Should().NotBe("This should not change");
        }

        [Fact]
        public async Task Test_Detached_Insert_User()
        {
            var user = new User
            {
                Id = "user",
                Name = "User",
                SetOnUpdate = "This is set on update",
                DontSetOnUpdate = "This is only set on insert"
            };

            await _users.Create(user);
            DetachData();

            user = await _users.GetById("user");
            user.SetOnUpdate.Should().Be("This is set on update");
            user.DontSetOnUpdate.Should().Be("This is only set on insert");
        }

        [Fact]
        public async Task Test_Detached_Update_User()
        {
            var user = new User
            {
                Id = "user",
                Name = "User",
                SetOnUpdate = "This is set on update",
                DontSetOnUpdate = "This is only set on insert"
            };

            await _users.Create(user);
            DetachData();

            user = await _users.GetById("user");
            user.SetOnUpdate.Should().Be("This is set on update");
            user.DontSetOnUpdate.Should().Be("This is only set on insert");

            user.SetOnUpdate = "This should be another value now";
            user.DontSetOnUpdate = "This should not change";

            await _users.Update(user);
            DetachData();

            user = await _users.GetById("user");
            user.SetOnUpdate.Should().Be("This should be another value now");
            user.DontSetOnUpdate.Should().NotBe("This should not change");
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
