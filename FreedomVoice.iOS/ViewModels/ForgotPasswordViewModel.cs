using System.Threading.Tasks;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class ForgotPasswordViewModel : BaseViewModel
    {
        private readonly IForgotPasswordService _service;

        private string _email;

        public const string EMailError = "Error message for email";

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public ForgotPasswordViewModel(UIViewController viewController, string email = "")
        {
            _service = ServiceContainer.Resolve<IForgotPasswordService>();

            ViewController = viewController;

            if (!string.IsNullOrEmpty(email)) EMail = email;
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
                ProceedErrorResponse(requestResult);
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
    }
}