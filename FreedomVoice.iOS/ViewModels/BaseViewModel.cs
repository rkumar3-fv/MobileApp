using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Events;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class BaseViewModel : PropertyChangedBase
    {
        protected UIViewController ViewController { private get; set; }

        protected BaseViewModel()
        {
            ProgressControl = ProgressControlType.ActivityIndicator;

            ProceedInitialFormValidation();
        }

        private void ProceedInitialFormValidation()
        {
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
        /// Protected method for validating the ViewModel - Fires PropertyChanged for IsValid and Errors
        /// </summary>
        protected virtual void Validate()
        {
            OnPropertyChanged("IsValid");
            OnPropertyChanged("Errors");
        }

        /// <summary>
        /// Other viewmodels should call this when overriding Validate, to validate each property
        /// </summary>
        /// <param name="validate">Func to determine if a value is valid</param>
        /// <param name="error">The error message to use if not valid</param>
        protected void ValidateProperty(Func<bool> validate, string error)
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
        protected bool IsBusy
        {
            private get { return _isBusy; }
            set
            {
                if (_isBusy == value) return;

                _isBusy = value;

                OnPropertyChanged("IsBusy");
                OnIsBusyChanged();
            }
        }

        public event EventHandler OnUnauthorizedResponse;
        public event EventHandler OnErrorConnectionResponse;
        public event EventHandler OnSuccessResponse;
        public event EventHandler OnPaymentRequiredResponse;
        public event EventHandler OnBadRequestResponse;
        public event EventHandler OnNotFoundResponse;
        public event EventHandler OnCanceledResponse;
        public event EventHandler OnForbiddenResponse;

        public bool IsErrorResponseReceived;

        protected void ProceedErrorResponse(BaseResponse baseResponse)
        {
            var response = baseResponse as ErrorResponse;
            if (response == null) return;

            IsErrorResponseReceived = true;

            switch (response.ErrorCode)
            {
                case ErrorResponse.ErrorPaymentRequired:
                    OnPaymentRequiredResponse?.Invoke(null, EventArgs.Empty);
                    return;
                case ErrorResponse.ErrorConnection:
                    new UIAlertView("Service is unavailable", "Please try again later.", null, "OK", null).Show();
                    OnErrorConnectionResponse?.Invoke(null, EventArgs.Empty);
                    return;
                case ErrorResponse.ErrorUnauthorized:
                    OnUnauthorizedResponse?.Invoke(null, EventArgs.Empty);
                    return;
                case ErrorResponse.ErrorBadRequest:
                    OnBadRequestResponse?.Invoke(null, EventArgs.Empty);
                    return;
                case ErrorResponse.ErrorNotFound:
                    OnNotFoundResponse?.Invoke(null, EventArgs.Empty);
                    return;
                case ErrorResponse.ErrorCancelled:
                    OnCanceledResponse?.Invoke(null, EventArgs.Empty);
                    return;
                case ErrorResponse.Forbidden:
                    OnForbiddenResponse?.Invoke(null, EventArgs.Empty);
                    return;
                case ErrorResponse.ErrorUnknown:
                    return;
            }
        }

        protected void ProceedSuccessResponse()
        {
            OnSuccessResponse?.Invoke(null, EventArgs.Empty);
        }

        protected static async Task RenewCookieIfNeeded()
        {
            if (Cookies.IsCookieStored())
                return;

            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            if (appDelegate != null)
                await appDelegate.ProceedAutoLogin();
        }

        protected ProgressControlType ProgressControl { private get; set; }

        private LoadingIndicator _loadingIndicator;

        private UIActivityIndicatorView _activityIndicator;

        public CGPoint ActivityIndicatorCenter { private get; set; }

        protected UIProgressView ProgressBar;
        protected UIButton CancelDownloadButton;

        private void OnIsBusyChanged()
        {
            if (!ViewController.IsViewLoaded)
                return;

            if (IsBusy)
            {
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

                _loadingIndicator = new LoadingIndicator(Theme.ScreenBounds, ProgressControl, ActivityIndicatorCenter);

                ProgressBar = _loadingIndicator.ProgressBar;
                CancelDownloadButton = _loadingIndicator.CancelDownloadButton;

                _activityIndicator = _loadingIndicator.ActivityIndicator;
                _activityIndicator?.StartAnimating();

                UIApplication.SharedApplication.KeyWindow.AddSubview(_loadingIndicator);
            }
            else
            {
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;

                _activityIndicator?.StopAnimating();
                _loadingIndicator?.Hide();
            }
        }
    }
}