using System.Threading.Tasks;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
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

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public LoginViewModel(UIViewController viewController)
        {
            _service = ServiceContainer.Resolve<ILoginService>();

            ViewController = viewController;
        }

        /// <summary>
        /// Constructor for autologin functionality, requires an IService
        /// </summary>
        public LoginViewModel(string userName, string password)
        {
            _service = ServiceContainer.Resolve<ILoginService>();

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
                ProceedErrorResponse(requestResult);
            else
            {
                KeyChain.SetPasswordForUsername(Username, Password);
                Cookies.SaveCookieToStore();

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
            var requestResult = await _service.ExecuteRequest(Username, Password);
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
                Cookies.SaveCookieToStore();
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
    }
}