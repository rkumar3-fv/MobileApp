using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IExtensionsService
    {
        /// <summary>
        /// Asynchronous retrieving of mailboxes
        /// </summary>
        Task<BaseResponse> ExecuteRequest(string systemNumber);
    }
}