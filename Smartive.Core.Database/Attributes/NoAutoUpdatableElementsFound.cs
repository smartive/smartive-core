using System;

namespace Smartive.Core.Database.Attributes
{
    /// <summary>
    /// Exception that is thrown when no auto updateable elements were found on a model type.
    /// </summary>
    public class NoAutoUpdatableElementsFound : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoAutoUpdatableElementsFound"/> class.
        /// </summary>
        /// <param name="type">T</param>
        public NoAutoUpdatableElementsFound(Type type)
            : base(
                $"There were no AutoUpdatable attributes found on type '{type}'")
        {
        }
    }
}
