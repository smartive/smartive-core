using System;
using Smartive.Core.Database.Repositories;

namespace Smartive.Core.Database.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Attribute that marks a class or a property of a class as auto-updateable.
    /// Use in conjunction with <see cref="T:Smartive.Core.Database.Repositories.EfAutoUpdateCrudRepository`2" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class AutoUpdatable : Attribute
    {
    }
}
