using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class ExtensionsViewModel : BaseViewModel
    {
        private readonly IExtensionsService _service;
        private readonly Account _selectedAccount;
        private readonly UINavigationController _viewController;

        private LoadingOverlay _loadingOverlay;

        public List<ExtensionWithCount> ExtensionsList { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public ExtensionsViewModel(Account selectedAccount, UINavigationController viewController)
        {
            ExtensionsList = new List<ExtensionWithCount>();

            _service = ServiceContainer.Resolve<IExtensionsService>();

            _selectedAccount = selectedAccount;
            _viewController = viewController;

            IsBusyChanged += OnIsBusyChanged;
            OnPaymentRequiredResponse += OnAccountPaymentRequired;
        }

        /// <summary>
        /// Performs an asynchronous Extensions With Count request
        /// </summary>
        /// <returns></returns>
        public async Task GetExtensionsListAsync()
        {
            IsBusy = true;

            _service.SetSystemNumber(_selectedAccount.PhoneNumber);

            var requestResult = await _service.ExecuteRequest();
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

        private void OnAccountPaymentRequired(object sender, EventArgs eventArgs)
        {
            var accountUnavailableController = AppDelegate.GetViewController<AccountUnavailableViewController>();
            accountUnavailableController.SelectedAccount = _selectedAccount;
            accountUnavailableController.ParentController = _viewController;

            var navigationController = new UINavigationController(accountUnavailableController);
            Theme.TransitionController(navigationController);
        }

        private void OnIsBusyChanged(object sender, EventArgs e)
        {
            if (!_viewController.IsViewLoaded)
                return;

            if (IsBusy)
            {
                _loadingOverlay = new LoadingOverlay(Theme.ScreenBounds);
                _viewController.View.Add(_loadingOverlay);
            }
            else
            {
                _loadingOverlay.Hide();
            }
        }
    }
}