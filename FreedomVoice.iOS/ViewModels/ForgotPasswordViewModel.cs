using System;
using System.Threading.Tasks;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class ForgotPasswordViewModel : BaseViewModel
    {
        private readonly IForgotPasswordService _service;

        private string _email;

        private readonly UIActivityIndicatorView _activityIndicator;
        private readonly UIViewController _viewController;

        public const string EMailError = "Error message for email";

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public ForgotPasswordViewModel(UIViewController viewController, UIActivityIndicatorView activityIndicator)
        {
            _service = ServiceContainer.Resolve<IForgotPasswordService>();

            ViewController = viewController;

            _viewController = viewController;
            _activityIndicator = activityIndicator;

            IsBusyChanged += OnIsBusyChanged;
        }

        /// <summary>
        /// Username property
        /// </summary>
        public string EMail
        {
            private get { return _email; }
            set
            {
                _email = value;
                Validate();
                OnPropertyChanged("EMail");
            }
        }

        /// <summary>
        /// Performs an asynchronous password reset
        /// </summary>
        /// <returns></returns>
        public async Task ForgotPasswordAsync()
        {
            IsBusy = true;

            var requestResult = await _service.ExecuteRequest(EMail);
            if (requestResult is ErrorResponse)
                await ProceedErrorResponse(requestResult);
            else
                ProceedSuccessResponse();

            IsBusy = false;
        }

        /// <summary>
        /// Validation logic
        /// </summary>
        protected override void Validate()
        {
            ValidateProperty(() => !Validation.IsValidEmail(EMail), EMailError);

            base.Validate();
        }

        private void OnIsBusyChanged(object sender, EventArgs e)
        {
            if (!_viewController.IsViewLoaded)
                return;

            if (IsBusy)
                _activityIndicator.StartAnimating();
            else
                _activityIndicator.StopAnimating();
        }
    }
}