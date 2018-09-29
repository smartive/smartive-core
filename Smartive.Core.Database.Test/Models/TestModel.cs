using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Test.Models
{
    public class TestModel : Base
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    
    public class TestModelKey : Base<string>
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
