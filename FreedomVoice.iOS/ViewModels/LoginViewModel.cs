using System;
using System.Threading.Tasks;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly ILoginService _service;

        private string _username;
        private string _password;

        public const string UsernameError = "Error message for username";
        public const string PasswordError = "Error message for password";

        private readonly UIActivityIndicatorView _activityIndicator;
        private readonly UIViewController _viewController;

        readonly TimeSpan _autoLogoutTime = TimeSpan.FromMinutes(2);
        DateTime _dateInactive = DateTime.Now;

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public LoginViewModel(UIViewController viewController, UIActivityIndicatorView activityIndicator)
        {
            _service = ServiceContainer.Resolve<ILoginService>();

            ViewController = viewController;

            _viewController = viewController;
            _activityIndicator = activityIndicator;

            IsBusyChanged += OnIsBusyChanged;
        }

        /// <summary>
        /// Username property
        /// </summary>
        public string Username
        {
            private get { return _username; }
            set
            {
                _username = value;
                Validate();
                OnPropertyChanged("Username");
            }
        }

        /// <summary>
        /// Password property
        /// </summary>
        public string Password
        {
            private get { return _password; }
            set
            {
                _password = value;
                Validate();
                OnPropertyChanged("Password");
            }
        }

        /// <summary>
        /// Performs an asynchronous login
        /// </summary>
        /// <returns></returns>
        public async Task LoginAsync()
        {
            IsBusy = true;

            var requestResult = await _service.ExecuteRequest(Username, Password);
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
                ProceedSuccessResponse();

            IsBusy = false;
        }

        /// <summary>
        /// True if the login screen should be presented
        /// </summary>
        public bool IsInactive => DateTime.Now - _dateInactive > _autoLogoutTime;

        /// <summary>
        /// Should be called to reset the last inactive time, so on DidEnterBackground on iOS, for example
        /// </summary>
        public void ResetInactiveTime()
        {
            _dateInactive = DateTime.Now;
        }

        /// <summary>
        /// Validation logic
        /// </summary>
        protected override void Validate()
        {
            ValidateProperty(() => !Validation.IsValidEmail(Username), UsernameError);
            ValidateProperty(() => !Validation.IsValidPassword(Password), PasswordError);

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