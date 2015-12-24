using System;
using System.Threading;
using System.Threading.Tasks;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;

namespace FreedomVoice.iOS.Services
{
    public interface IMediaService
    {
        /// <summary>
        /// Asynchronous retrieving of media
        /// </summary>
        Task<BaseResponse> ExecuteRequest(IProgress<DownloadBytesProgress> progressReporter, string systemNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType, string filePath, CancellationToken token);
    }
}