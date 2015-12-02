using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.ViewModels
{
    public class AccountsViewModel : BaseViewModel
    {
        readonly IAccountsService _service;

        public List<Account> AccountsList { get; private set; }

        public bool DoNotUseCache { private get; set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public AccountsViewModel()
        {
            AccountsList = new List<Account>();

            _service = ServiceContainer.Resolve<IAccountsService>();
        }

        /// <summary>
        /// Performs an asynchronous login
        /// </summary>
        /// <returns></returns>
        public async Task GetAccountsListAsync()
        {
            await RenewCookieIfNeeded();

            var requestResult = await _service.ExecuteRequest(DoNotUseCache);
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as AccountsResponse;
                if (data != null)
                    AccountsList = data.AccountsList;
            }
        }
    }
}