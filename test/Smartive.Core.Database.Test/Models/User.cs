using System.Collections.Generic;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Test.Models
{
    public class User : Base<string>
    {
        public string Email => Id;

        public string Name { get; set; }

        public IList<UserItem> UserItems { get; set; }
    }
}
