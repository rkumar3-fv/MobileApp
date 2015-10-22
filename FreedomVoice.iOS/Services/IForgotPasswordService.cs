using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IForgotPasswordService
    {
        void SetRecoveryEmail(string email);

        /// <summary>
        /// Asynchronous forgot password request
        /// </summary>
        Task<BaseResponse> ExecuteRequest();
    }
}