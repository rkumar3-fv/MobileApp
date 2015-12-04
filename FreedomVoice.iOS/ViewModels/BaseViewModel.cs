using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities.Events;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public abstract class BaseViewModel : PropertyChangedBase
    {
        protected abstract string ResponseName { get; set; }

        protected BaseViewModel()
        {
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
        public event EventHandler OnInternalErrorResponse;

        public bool IsErrorResponseReceived;

        protected string ProceedErrorResponse(BaseResponse baseResponse)
        {
            var response = baseResponse as ErrorResponse;
            if (response == null) return string.Empty;

            IsErrorResponseReceived = true;

            switch (response.ErrorCode)
            {
                case ErrorResponse.ErrorBadRequest:
                    OnBadRequestResponse?.Invoke(null, EventArgs.Empty);
                    return "400 - Bad Request";
                case ErrorResponse.ErrorCancelled:
                    OnCanceledResponse?.Invoke(null, EventArgs.Empty);
                    return "Cancelled";
                case ErrorResponse.ErrorConnection:
                case ErrorResponse.ErrorGatewayTimeout:
                case ErrorResponse.ErrorRequestTimeout:
                    if (OnErrorConnectionResponse == null)
                        new UIAlertView("Service is unavailable", "Please try again later.", null, "OK", null).Show();
                    OnErrorConnectionResponse?.Invoke(null, EventArgs.Empty);
                    return response.ErrorCode == ErrorResponse.ErrorConnection ? "Connection Lost" : response.ErrorCode == ErrorResponse.ErrorGatewayTimeout ? "504 - Gateway Timeout" : "408 - Request Timeout";
                case ErrorResponse.ErrorUnauthorized:
                    OnUnauthorizedResponse?.Invoke(null, EventArgs.Empty);
                    return "401 - Unauthorized";
                case ErrorResponse.ErrorPaymentRequired:
                    OnPaymentRequiredResponse?.Invoke(null, EventArgs.Empty);
                    return "402 - Payment Required";
                case ErrorResponse.ErrorForbidden:
                    OnForbiddenResponse?.Invoke(null, EventArgs.Empty);
                    return "403 - Forbidden";
                case ErrorResponse.ErrorNotFound:
                    OnNotFoundResponse?.Invoke(null, EventArgs.Empty);
                    return "404 - Not Found";
                case ErrorResponse.ErrorInternal:
                case ErrorResponse.ErrorUnknown:
                    if (OnInternalErrorResponse == null)
                        new UIAlertView("Internal server error", "Please try again later.", null, "OK", null).Show();
                    OnInternalErrorResponse?.Invoke(null, EventArgs.Empty);
                    return response.ErrorCode == ErrorResponse.ErrorInternal ? "500 - Internal Server Error" : "Unknown Error";
                default:
                    return "Unknown Error";
            }
        }

        private Stopwatch _watcher;
        protected void StartWatcher()
        {
            _watcher = Stopwatch.StartNew();
        }

        protected void StopWatcher(string errorResponse)
        {
            _watcher.Stop();
            Log.ReportTime(Log.EventCategory.Request, ResponseName, string.IsNullOrEmpty(errorResponse) ? ResponseName : $"{ResponseName}: {errorResponse}", _watcher.ElapsedMilliseconds);
        }

        protected void ProceedSuccessResponse()
        {
            OnSuccessResponse?.Invoke(null, EventArgs.Empty);
        }

        protected static async Task RenewCookieIfNeeded()
        {
            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            if (appDelegate != null)
                await appDelegate.PrepareAuthentificationCookie();
        }

        protected virtual ProgressControlType ProgressControl => ProgressControlType.ActivityIndicator;

        private void OnIsBusyChanged()
        {
            if (!UIApplication.SharedApplication.KeyWindow.RootViewController.IsViewLoaded)
                return;

            if (IsBusy)
            {
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

                if (ProgressControl == ProgressControlType.ActivityIndicator)
                {
                    UIApplication.SharedApplication.KeyWindow.AddSubview(AppDelegate.ActivityIndicator);
                    AppDelegate.ActivityIndicator.Show();
                }
                else
                {
                    UIApplication.SharedApplication.KeyWindow.AddSubview(AppDelegate.DownloadIndicator);
                    AppDelegate.DownloadIndicator.Show();
                }
            }
            else
            {
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;

                if (ProgressControl == ProgressControlType.ActivityIndicator)
                    AppDelegate.ActivityIndicator.Hide();
                else
                    AppDelegate.DownloadIndicator.Hide();
            }
        }
    }

    public enum ProgressControlType
    {
        ProgressBar,
        ActivityIndicator
    }
}