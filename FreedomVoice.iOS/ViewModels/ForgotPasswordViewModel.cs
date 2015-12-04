using System.Threading.Tasks;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities.Helpers;

namespace FreedomVoice.iOS.ViewModels
{
    public class ForgotPasswordViewModel : BaseViewModel
    {
        public const string EMailError = "Error message for email";

        protected override string ResponseName
        {
            get { return "ForgotPassword"; }
            set { }
        }

        private readonly IForgotPasswordService _service;

        private string _email;

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public ForgotPasswordViewModel(string email = "")
        {
            _service = ServiceContainer.Resolve<IForgotPasswordService>();

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

            StartWatcher();

            var errorResponse = string.Empty;
            var requestResult = await _service.ExecuteRequest(EMail);
            if (requestResult is ErrorResponse)
                errorResponse = ProceedErrorResponse(requestResult);
            else
                ProceedSuccessResponse();

            StopWatcher(errorResponse);

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