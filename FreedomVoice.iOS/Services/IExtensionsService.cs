using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IExtensionsService
    {
        void SetSystemNumber(string systemNumber);

        /// <summary>
        /// Asynchronous retrieving of mailboxes
        /// </summary>
        Task<BaseResponse> ExecuteRequest();
    }
}