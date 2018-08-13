using System;
using Smartive.Core.Database.Repositories;

namespace Smartive.Core.Database.Attributes
{
    /// <summary>
    /// Attribute that marks a class or a property of a class as autoupdateable.
    /// Use in conjunction with <see cref="EfAutoUpdateCrudRepository{TKey,TEntity}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class AutoUpdatable : Attribute
    {
    }
}
