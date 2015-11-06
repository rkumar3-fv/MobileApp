using System.Threading;
using System.Threading.Tasks;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IMediaService
    {
        /// <summary>
        /// Asynchronous retrieving of media
        /// </summary>
        Task<BaseResponse> ExecuteRequest(string systemNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType, CancellationToken token);
    }
}