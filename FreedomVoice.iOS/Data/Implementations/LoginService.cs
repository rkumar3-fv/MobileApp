using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities.Base;

namespace FreedomVoice.iOS.Data.Implementations
{
    public class LoginService : ILoginService
    {
        public Task<BaseResult<string>> LoginAsync(string username, string password)
        {
            return Task.Factory.StartNew(async () => await ApiHelper.Login(username, password)).Unwrap();
        }
    }
}