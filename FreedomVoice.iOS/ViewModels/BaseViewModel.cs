using System;
using System.Collections.Generic;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using UIKit;

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

            IsValidChanged?.Invoke(this, EventArgs.Empty);
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

        private bool _isBusy;

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
            IsBusyChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event for successfull response
        /// </summary>
        public event EventHandler OnSuccessResponse;

        /// <summary>
        /// Event for unauthorized response
        /// </summary>
        public event EventHandler OnUnauthorizedResponse;

        /// <summary>
        /// Event for payment required response
        /// </summary>
        public event EventHandler OnPaymentRequiredResponse;

        /// <summary>
        /// Event for payment required response
        /// </summary>
        public event EventHandler OnBadRequestResponse;

        protected void ProceedErrorResponse(BaseResponse baseResponse)
        {
            var response = baseResponse as ErrorResponse;
            if (response == null) return;

            switch (response.ErrorCode)
            {
                case ErrorResponse.PaymentRequired:
                    OnPaymentRequiredResponse?.Invoke(null, EventArgs.Empty);
                    return;
                case ErrorResponse.ErrorConnection:
                    new UIAlertView("Service is unreachable", "Please try again later.", null, "OK", null).Show();
                    return;
                case ErrorResponse.ErrorUnauthorized:
                    OnUnauthorizedResponse?.Invoke(null, EventArgs.Empty);
                    return;
                case ErrorResponse.ErrorBadRequest:
                    OnBadRequestResponse?.Invoke(null, EventArgs.Empty);
                    return;
                case ErrorResponse.ErrorUnknown:
                    return;
            }
        }

        protected void ProceedSuccessResponse()
        {
            OnSuccessResponse?.Invoke(null, EventArgs.Empty);
        }
    }
}