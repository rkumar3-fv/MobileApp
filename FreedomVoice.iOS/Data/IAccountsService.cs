using System.Threading.Tasks;
using FreedomVoice.Core.Entities;
using FreedomVoice.Core.Entities.Base;

namespace FreedomVoice.iOS.Data
{
    public interface IAccountsService
    {
        /// <summary>
        /// Asynchronous retrieving of accounts
        /// </summary>
        Task<BaseResult<DefaultPhoneNumbers>> GetAccountsListAsync();
    }
}