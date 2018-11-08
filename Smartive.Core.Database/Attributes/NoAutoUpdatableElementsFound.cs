using System;

namespace Smartive.Core.Database.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Exception that is thrown when no auto updateable elements were found on a model type.
    /// </summary>
    public class NoAutoUpdatableElementsFound : Exception
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Smartive.Core.Database.Attributes.NoAutoUpdateableElementsFound" /> class.
        /// </summary>
        /// <param name="type">The type that was searched for updateable properties.</param>
        public NoAutoUpdatableElementsFound(Type type)
            : base(
                $"There were no AutoUpdateable attributes found on type '{type}'")
        {
        }
    }
}
