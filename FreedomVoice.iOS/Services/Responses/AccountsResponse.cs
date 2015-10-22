using System.Collections.Generic;
using FreedomVoice.iOS.Entities;

namespace FreedomVoice.iOS.Services.Responses
{
    public class AccountsResponse : BaseResponse
    {
        public List<Account> AccountsList { get; }

        /// <summary>
        /// Response init for AccountsService
        /// </summary>
        /// <param name="accounts">Accounts</param>
        public AccountsResponse(IEnumerable<string> accounts)
        {
            AccountsList = new List<Account>();

            foreach (var account in accounts)
                AccountsList.Add(new Account(account));
        }
    }
}