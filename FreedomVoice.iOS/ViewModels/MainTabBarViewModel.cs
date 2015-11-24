using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class MainTabBarViewModel : BaseViewModel
    {
        private readonly IPresentationNumbersService _presentationNumbersService;
        private readonly IPollingIntervalService _poolingIntervalService;

        private readonly Account _selectedAccount;
        private readonly UIViewController _viewController;

        public List<PresentationNumber> PresentationNumbers { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public MainTabBarViewModel(Account selectedAccount, UIViewController viewController)
        {
            _presentationNumbersService = ServiceContainer.Resolve<IPresentationNumbersService>();
            _poolingIntervalService = ServiceContainer.Resolve<IPollingIntervalService>();

            _selectedAccount = selectedAccount;
            _viewController = viewController;
            ViewController = viewController;

            OnPaymentRequiredResponse += OnAccountPaymentRequired;
        }

        /// <summary>
        /// Performs an asynchronous request of presentation phones
        /// </summary>
        /// <returns></returns>
        public async Task GetPresentationNumbersAsync()
        {
            IsBusy = true;

            var requestResult = await _presentationNumbersService.ExecuteRequest(_selectedAccount.PhoneNumber);
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as PresentationNumbersResponse;
                if (data != null)
                    PresentationNumbers = data.PresentationNumbers;
            }
        }

        /// <summary>
        /// Performs an asynchronous Extensions With Count request
        /// </summary>
        /// <returns></returns>
        public async Task GetPoolingIntervalAsync()
        {
            var requestResult = await _poolingIntervalService.ExecuteRequest();
            if (requestResult is ErrorResponse)
            {
                ProceedErrorResponse(requestResult);
                IsBusy = false;
            }
            else
            {
                var data = requestResult as PollingIntervalResponse;
                if (data != null)
                    UserDefault.PoolingInterval = data.PollingInterval;
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