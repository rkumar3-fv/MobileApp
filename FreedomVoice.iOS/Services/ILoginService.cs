using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface ILoginService
    {
        void SetCredentials(string userName, string password);

        /// <summary>
        /// Asynchronous login
        /// </summary>
        Task<BaseResponse> ExecuteRequest();
    }
}