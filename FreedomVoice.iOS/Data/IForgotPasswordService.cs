using System.Threading.Tasks;
using FreedomVoice.Core.Entities.Base;

namespace FreedomVoice.iOS.Data
{
    public interface IForgotPasswordService
    {
        /// <summary>
        /// Asynchronous forgot password request
        /// </summary>
        Task<BaseResult<string>> ForgotPasswordAsync(string email);
    }
}