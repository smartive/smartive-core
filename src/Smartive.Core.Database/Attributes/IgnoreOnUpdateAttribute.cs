using System;
using Smartive.Core.Database.Repositories;

namespace Smartive.Core.Database.Attributes
{
    /// <summary>
    /// Declares the given property as "ignored during update actions". When the save or update
    /// method of the default <see cref="EfCrudRepository{TKey,TEntity,TContext}"/> is called, those properties
    /// are excluded from saving their values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreOnUpdateAttribute : Attribute
    {
    }
}
