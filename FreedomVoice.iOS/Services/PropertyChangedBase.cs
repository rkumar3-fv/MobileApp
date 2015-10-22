using System.ComponentModel;

namespace FreedomVoice.iOS.Services
{
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Event for INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Protected method for firing PropertyChanged
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var method = PropertyChanged;
            method?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}