using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IFoldersService
    {
        void SetParameters(string systemNumber, int mailboxNumber);

        /// <summary>
        /// Asynchronous retrieving of folders with count
        /// </summary>
        Task<BaseResponse> ExecuteRequest();
    }
}