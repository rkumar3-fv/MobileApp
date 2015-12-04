using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;

namespace FreedomVoice.iOS.ViewModels
{
    public class ExtensionsViewModel : BaseViewModel
    {
        protected override string ResponseName
        {
            get { return "GetExtensions"; }
            set { }
        }

        private readonly IExtensionsService _service;

        private readonly Account _selectedAccount;

        public List<ExtensionWithCount> ExtensionsList { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public ExtensionsViewModel(Account selectedAccount)
        {
            ExtensionsList = new List<ExtensionWithCount>();

            _service = ServiceContainer.Resolve<IExtensionsService>();

            _selectedAccount = selectedAccount;
        }

        /// <summary>
        /// Performs an asynchronous Extensions With Count request
        /// </summary>
        /// <returns></returns>
        public async Task GetExtensionsListAsync(bool silent = false)
        {
            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            if (!silent)
                IsBusy = true;

            StartWatcher();

            await RenewCookieIfNeeded();

            var errorResponse = string.Empty;
            var requestResult = await _service.ExecuteRequest(_selectedAccount.PhoneNumber);
            if (requestResult is ErrorResponse)
            {
                if (!silent)
                    errorResponse = ProceedErrorResponse(requestResult);
            }
            else
            {
                var data = requestResult as ExtensionsWithCountResponse;
                if (data != null)
                    ExtensionsList = data.ExtensionsWithCount;
            }
            
            StopWatcher(errorResponse);

            if (!silent)
                IsBusy = false;
        }
    }
}