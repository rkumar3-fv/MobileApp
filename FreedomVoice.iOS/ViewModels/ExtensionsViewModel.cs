using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class ExtensionsViewModel : BaseViewModel
    {
        private readonly IExtensionsService _service;
        private readonly UIViewController _viewController;

        private readonly Account _selectedAccount;

        public List<ExtensionWithCount> ExtensionsList { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public ExtensionsViewModel(Account selectedAccount, UIViewController viewController)
        {
            ExtensionsList = new List<ExtensionWithCount>();

            _service = ServiceContainer.Resolve<IExtensionsService>();

            ViewController = viewController;
            _viewController = viewController;

            _selectedAccount = selectedAccount;
        }

        /// <summary>
        /// Performs an asynchronous Extensions With Count request
        /// </summary>
        /// <returns></returns>
        public async Task GetExtensionsListAsync()
        {
            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(_viewController, Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            IsBusy = true;

            await RenewCookieIfNeeded();

            var requestResult = await _service.ExecuteRequest(_selectedAccount.PhoneNumber);
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as ExtensionsWithCountResponse;
                if (data != null)
                    ExtensionsList = data.ExtensionsWithCount;
            }

            IsBusy = false;
        }
    }
}