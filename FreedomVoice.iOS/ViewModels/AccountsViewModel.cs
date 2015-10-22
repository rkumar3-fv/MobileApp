﻿using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;

namespace FreedomVoice.iOS.ViewModels
{
    public class AccountsViewModel : BaseViewModel
    {
        readonly IAccountsService _service;

        public List<Account> AccountsList { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public AccountsViewModel()
        {
            _service = ServiceContainer.Resolve<IAccountsService>();
            AccountsList = new List<Account>();
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
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as AccountsResponse;
                if (data != null)
                    AccountsList = data.AccountsList;
            }

            IsBusy = false;
        }
    }
}