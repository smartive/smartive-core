using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Test.Models
{
    public class Book : Base
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int AuthorId { get; set; }

        public Author Author { get; set; }

        public IList<Comment> Comments { get; set; }
    }
}
