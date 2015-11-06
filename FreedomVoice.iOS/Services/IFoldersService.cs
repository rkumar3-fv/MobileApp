using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IFoldersService
    {
        /// <summary>
        /// Asynchronous retrieving of folders with count
        /// </summary>
        Task<BaseResponse> ExecuteRequest(string systemNumber, int mailboxNumber);
    }
}