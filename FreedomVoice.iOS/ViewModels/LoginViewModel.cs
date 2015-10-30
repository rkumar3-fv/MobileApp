using System;
using System.Threading.Tasks;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;

namespace FreedomVoice.iOS.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        readonly ILoginService _service;

        string _username;
        string _password;

        public const string UsernameError = "Error message for username";
        public const string PasswordError = "Error message for password";

        readonly TimeSpan _autoLogoutTime = TimeSpan.FromMinutes(2);
        DateTime _dateInactive = DateTime.Now;

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public LoginViewModel()
        {
            _service = ServiceContainer.Resolve<ILoginService>();
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

            _service.SetCredentials(Username, Password);

            var requestResult = await _service.ExecuteRequest();
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
    }
}