using System.Threading.Tasks;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.iOS.Data;
using FreedomVoice.iOS.Helpers;
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
            get { return _email; }
            set
            {
                _email = value;
                Validate();
                OnPropertyChanged("EMail");
            }
        }

        /// <summary>
        /// Performs an asynchronous login
        /// </summary>
        /// <returns></returns>
        public Task<BaseResult<string>> ForgotPasswordAsync()
        {
            IsBusy = true;
            return _service.ForgotPasswordAsync(EMail)
                           .ContinueOnCurrentThread(t => {
                               IsBusy = false;
                               return t.Result;
                           });
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