using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IMessageOperationsService
    {
        /// <summary>
        /// Asynchronously move messages to Trash folder
        /// </summary>
        Task<BaseResponse> ExecuteMoveRequest(string systemNumber, int mailboxNumber, string destinationFolder, List<string> messageIds);

        /// <summary>
        /// Asynchronously delete messages
        /// </summary>
        Task<BaseResponse> ExecuteDeleteRequest(string systemNumber, int mailboxNumber, List<string> messageIds);
    }
}