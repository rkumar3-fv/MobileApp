using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IPresentationNumbersService
    {
        void SetSystemNumber(string systemNumber);

        /// <summary>
        /// Asynchronous retrieving of presentation phone numbers
        /// </summary>
        Task<BaseResponse> ExecuteRequest();
    }
}