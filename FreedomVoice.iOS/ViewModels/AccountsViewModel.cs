using System.Threading.Tasks;
using FreedomVoice.Core.Entities;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.iOS.Data;
using FreedomVoice.iOS.Utilities;

namespace FreedomVoice.iOS.ViewModels
{
    public class AccountsViewModel : BaseViewModel
    {
        readonly IAccountsService _service;

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public AccountsViewModel()
        {
            _service = ServiceContainer.Resolve<IAccountsService>();
        }

        /// <summary>
        /// Performs an asynchronous login
        /// </summary>
        /// <returns></returns>
        public Task<BaseResult<DefaultPhoneNumbers>> GetAccountsListAsync()
        {
            IsBusy = true;
            return _service.GetAccountsListAsync()
                           .ContinueOnCurrentThread(t => {
                               IsBusy = false;
                               return t.Result;
                           });
        }
    }
}
