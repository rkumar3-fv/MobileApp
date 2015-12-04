using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class PresentationPhonesViewModel : BaseViewModel
    {
        private readonly IPresentationNumbersService _presentationNumbersService;

        private readonly Account _selectedAccount;
        private readonly UIViewController _viewController;

        public List<PresentationNumber> PresentationNumbers { get; private set; }

        public bool DoNotUseCache { private get; set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public PresentationPhonesViewModel(Account selectedAccount, UIViewController viewController)
        {
            _presentationNumbersService = ServiceContainer.Resolve<IPresentationNumbersService>();

            _selectedAccount = selectedAccount;
            _viewController = viewController;

            OnPaymentRequiredResponse += OnAccountPaymentRequired;
        }

        /// <summary>
        /// Performs an asynchronous request of presentation phones
        /// </summary>
        /// <returns></returns>
        public async Task GetPresentationNumbersAsync()
        {
            IsBusy = true;

            var requestResult = await _presentationNumbersService.ExecuteRequest(_selectedAccount.PhoneNumber, DoNotUseCache);
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as PresentationNumbersResponse;
                if (data != null)
                    PresentationNumbers = data.PresentationNumbers;
            }

            IsBusy = false;
        }

        private void OnAccountPaymentRequired(object sender, EventArgs eventArgs)
        {
            var accountUnavailableController = AppDelegate.GetViewController<AccountUnavailableViewController>();
            accountUnavailableController.SelectedAccount = _selectedAccount;
            accountUnavailableController.ParentController = _viewController as UINavigationController;

            var navigationController = new UINavigationController(accountUnavailableController);
            Theme.TransitionController(navigationController);
        }
    }
}