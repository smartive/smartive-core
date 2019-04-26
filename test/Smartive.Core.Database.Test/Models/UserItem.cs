using System.ComponentModel.DataAnnotations;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Test.Models
{
    public class UserItem : Base
    {
        public string Name { get; set; }

        [Required]
        public string UserId { get; set; }

        public User User { get; set; }
    }
}
