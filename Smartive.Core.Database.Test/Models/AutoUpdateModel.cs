using Smartive.Core.Database.Attributes;
using Smartive.Core.Database.Models;

namespace Smartive.Core.Database.Test.Models
{
    [AutoUpdatable]
    public class AutoUpdateModel : Base
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class AutoPropertyUpdateModel : Base
    {
        [AutoUpdatable]
        public string Name { get; set; }
        [AutoUpdatable]
        public int Age { get; set; }
    }
}
