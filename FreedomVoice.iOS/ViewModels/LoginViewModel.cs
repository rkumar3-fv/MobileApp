using System;
using System.Threading.Tasks;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
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
        /// Constructor for autologin functionality, requires an IService
        /// </summary>
        public LoginViewModel(string userName, string password, UIViewController viewController)
        {
            _service = ServiceContainer.Resolve<ILoginService>();

            //TODO: Remove after functional implementation
            _viewController = viewController;

            Username = userName;
            Password = password;
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
                await ProceedErrorResponse(requestResult);
            else
            {
                KeyChain.SetPasswordForUsername(Username, Password);
                ProceedSuccessResponse();
            }

            IsBusy = false;
        }

        /// <summary>
        /// Performs an asynchronous login
        /// </summary>
        /// <returns></returns>
        public async Task AutoLoginAsync()
        {
            //TODO: Remove after functional implementation
            //var alertController = UIAlertController.Create(null, "Auto Login process was executed.", UIAlertControllerStyle.Alert);
            //alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
            //_viewController.PresentViewController(alertController, true, null);

            var requestResult = await _service.ExecuteRequest(Username, Password);
            if (requestResult is ErrorResponse)
                await ProceedErrorResponse(requestResult);
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