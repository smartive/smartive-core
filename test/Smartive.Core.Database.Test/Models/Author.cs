using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Test.Models
{
    public class Author : Base
    {
        [Required]
        public string Name { get; set; }

        public int Age { get; set; }

        public IList<Book> Books { get; set; }
    }
}
