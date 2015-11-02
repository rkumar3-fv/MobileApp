using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IMessagesService
    {
        /// <summary>
        /// Asynchronous retrieving of messages
        /// </summary>
        Task<BaseResponse> ExecuteRequest(string systemNumber, int mailboxNumber, string folderName);
    }
}