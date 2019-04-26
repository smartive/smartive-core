using System.ComponentModel.DataAnnotations;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Test.Models
{
    public class Comment : Base
    {
        [Required]
        public string Message { get; set; }

        [Required]
        public int BookId { get; set; }

        public Book Book { get; set; }
    }
}
