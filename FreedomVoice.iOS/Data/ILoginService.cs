using System.Threading.Tasks;
using FreedomVoice.Core.Entities.Base;

namespace FreedomVoice.iOS.Data
{
    public interface ILoginService
    {
        /// <summary>
        /// Asynchronous login
        /// </summary>
        Task<BaseResult<string>> LoginAsync(string username, string password);
    }
}