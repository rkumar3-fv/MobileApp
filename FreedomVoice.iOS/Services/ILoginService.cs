using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface ILoginService
    {
        /// <summary>
        /// Asynchronous login
        /// </summary>
        Task<BaseResponse> ExecuteRequest(string userName, string password);
    }
}