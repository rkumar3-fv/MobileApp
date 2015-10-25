using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IMessagesService
    {
        void SetParameters(string systemNumber, int mailboxNumber, string folderName);

        /// <summary>
        /// Asynchronous retrieving of messages
        /// </summary>
        Task<BaseResponse> ExecuteRequest();
    }
}