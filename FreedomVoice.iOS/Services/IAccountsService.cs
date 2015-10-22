using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IAccountsService
    {
        /// <summary>
        /// Asynchronous retrieving of accounts
        /// </summary>
        Task<BaseResponse> ExecuteRequest();
    }
}