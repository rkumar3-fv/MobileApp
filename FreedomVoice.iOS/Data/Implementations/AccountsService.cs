using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities;
using FreedomVoice.Core.Entities.Base;

namespace FreedomVoice.iOS.Data.Implementations
{
    public class AccountsService : IAccountsService
    {
        public Task<BaseResult<DefaultPhoneNumbers>> GetAccountsListAsync()
        {
            return Task.Factory.StartNew(async () => await ApiHelper.GetSystems()).Unwrap();
        }
    }
}