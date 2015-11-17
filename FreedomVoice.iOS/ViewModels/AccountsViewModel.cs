using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class AccountsViewModel : BaseViewModel
    {
        readonly IAccountsService _service;

        public List<Account> AccountsList { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public AccountsViewModel(UIViewController viewController)
        {
            AccountsList = new List<Account>();

            _service = ServiceContainer.Resolve<IAccountsService>();

            ViewController = viewController;
        }

        /// <summary>
        /// Performs an asynchronous login
        /// </summary>
        /// <returns></returns>
        public async Task GetAccountsListAsync()
        {
            IsBusy = true;

            var requestResult = await _service.ExecuteRequest();
            if (requestResult is ErrorResponse)
                await ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as AccountsResponse;
                if (data != null)
                    AccountsList = data.AccountsList;
            }

            IsBusy = false;

            //IsBusy = true;

            //int failureThreshold = 1;

            //do
            //{
            //    var requestResult = await _service.ExecuteRequest();
            //    if (requestResult is ErrorResponse)
            //        await ProceedErrorResponse(requestResult);
            //    else
            //    {
            //        failureThreshold = 0;
            //        var data = requestResult as AccountsResponse;
            //        if (data != null)
            //            AccountsList = data.AccountsList;
            //    }

            //    if (failureThreshold == 0)
            //    {
            //        await Task.Yield();
            //        break;
            //    }

            //    failureThreshold--;
            //} while (true);

            //IsBusy = false;
        }
    }
}