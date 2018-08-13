using System.ComponentModel.DataAnnotations;

namespace Smartive.Core.Database.Models
{
#pragma warning disable SA1402

    /// <summary>
    /// Basic model class for entities.
    /// </summary>
    /// <typeparam name="TKey">The type of the id property.</typeparam>
    public abstract class Base<TKey>
    {
        /// <summary>
        /// Gets or sets the id property. The type is defined by TKey.
        /// </summary>
        [Key]
        public TKey Id { get; set; }
    }

    /// <inheritdoc />
    /// <summary>
    /// Default basic model class which an `int` Id (Tkey)
    /// </summary>
    public abstract class Base : Base<int>
    {
    }

#pragma warning restore SA1402
}
