using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Smartive.Core.Wpf.ViewModels
{
    /// <summary>
    /// Abstract BaseViewModel that implements <see cref="INotifyPropertyChanged"/> interface
    /// to set fields and notify changes.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc />
        /// <summary>
        /// Public property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to set a referenced field and notify the change
        /// through the notify property changed event.
        /// </summary>
        /// <param name="field">Reference to the field that should be set</param>
        /// <param name="value">The new value of the field</param>
        /// <param name="propertyName">The property-name that will be set (compiler guessed)</param>
        /// <typeparam name="T">Type of the field / value</typeparam>
        /// <returns>True if the field was set (because the last value was not equal the current), false otherwise</returns>
        protected bool SetField<T>(
            ref T field,
            T value,
            [CallerMemberName] string propertyName = default)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Event method to notify a list of properties as changed.
        /// </summary>
        /// <param name="properties">List of properties that changed.</param>
        protected void OnPropertyChanged(params string[] properties)
        {
            foreach (var property in properties)
            {
                OnPropertyChanged(property);
            }
        }

        /// <summary>
        /// Event method to notify a property as changed.
        /// </summary>
        /// <param name="propertyName">Name of the property (compiler guessed)</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = default)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
