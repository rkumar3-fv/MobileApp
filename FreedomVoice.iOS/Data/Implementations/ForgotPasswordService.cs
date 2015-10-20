using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities.Base;

namespace FreedomVoice.iOS.Data.Implementations
{
    public class ForgotPasswordService : IForgotPasswordService
    {
        public Task<BaseResult<string>> ForgotPasswordAsync(string email)
        {
            return Task.Factory.StartNew(async () => await ApiHelper.PasswordReset(email)).Unwrap();
        }
    }
}