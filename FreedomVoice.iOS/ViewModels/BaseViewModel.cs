using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreedomVoice.iOS.Data;

namespace FreedomVoice.iOS.ViewModels
{
    public class BaseViewModel : PropertyChangedBase
    {
        /// <summary>
        /// Event for when IsBusy changes
        /// </summary>
        public event EventHandler IsBusyChanged;

        /// <summary>
        /// Event for when IsValid changes
        /// </summary>
        public event EventHandler IsValidChanged;

        /// <summary>
        /// Default constructor
        /// </summary>
        protected BaseViewModel()
        {
            //Make sure validation is performed on startup
            Validate();
        }

        /// <summary>
        /// Returns true if the current state of the ViewModel is valid
        /// </summary>
        public bool IsValid => Errors.Count == 0;

        /// <summary>
        /// A list of errors if IsValid is false
        /// </summary>
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Protected method for validating the ViewModel
        /// - Fires PropertyChanged for IsValid and Errors
        /// </summary>
        protected virtual void Validate()
        {
            OnPropertyChanged("IsValid");
            OnPropertyChanged("Errors");

            var method = IsValidChanged;
            method?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Other viewmodels should call this when overriding Validate, to validate each property
        /// </summary>
        /// <param name="validate">Func to determine if a value is valid</param>
        /// <param name="error">The error message to use if not valid</param>
        protected virtual void ValidateProperty(Func<bool> validate, string error)
        {
            if (validate())
            {
                if (!Errors.Contains(error))
                    Errors.Add(error);
            }
            else
            {
                Errors.Remove(error);
            }
        }

        bool _isBusy;

        /// <summary>
        /// Value inidicating if a spinner should be shown
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value) return;

                _isBusy = value;

                OnPropertyChanged("IsBusy");
                OnIsBusyChanged();
            }
        }

        /// <summary>
        /// Other viewmodels can override this if something should be done when busy
        /// </summary>
        private void OnIsBusyChanged()
        {
            var method = IsBusyChanged;
            if (method != null)
            {
                IsBusyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}