using System.Threading.Tasks;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;

namespace FreedomVoice.iOS.ViewModels
{
    public class ForgotPasswordViewModel : BaseViewModel
    {
        readonly IForgotPasswordService _service;

        string _email;

        public const string EMailError = "Error message for email";

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public ForgotPasswordViewModel()
        {
            _service = ServiceContainer.Resolve<IForgotPasswordService>();
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
            _service.SetRecoveryEmail(EMail);

            var requestResult = await _service.ExecuteRequest();
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
                ProceedSuccessResponse();
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