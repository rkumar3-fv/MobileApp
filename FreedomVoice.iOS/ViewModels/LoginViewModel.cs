using System;
using System.Threading.Tasks;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.iOS.Data;
using FreedomVoice.iOS.Helpers;
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
            get { return _username; }
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
            get { return _password; }
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
        public Task<BaseResult<string>> LoginAsync()
        {
            IsBusy = true;
            return _service.LoginAsync(Username, Password)
                           .ContinueOnCurrentThread(t => {
                                IsBusy = false;
                                return t.Result;
                            });
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