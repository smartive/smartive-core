using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Smartive.Core.Database.Models
{
#pragma warning disable SA1402

    /// <summary>
    /// Basic model class for entities.
    /// </summary>
    /// <typeparam name="TKey">The type of the id property.</typeparam>
    public abstract class Base<TKey>
        where TKey : notnull
    {
        /// <summary>
        /// Gets or sets the id property. The type is defined by TKey.
        /// </summary>
        [Key]
        [NotNull]
        public TKey Id { get; set; } = default(TKey) !;
    }

    /// <inheritdoc />
    /// <summary>
    /// Default basic model class which an `int` Id (TKey).
    /// </summary>
    public abstract class Base : Base<int>
    {
    }

#pragma warning restore SA1402
}
