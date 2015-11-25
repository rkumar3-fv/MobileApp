using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class MediaService : BaseService, IMediaService
    {
        public async Task<BaseResponse> ExecuteRequest(IProgress<DownloadBytesProgress> progressReporter, string systemNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType, CancellationToken token)
        {
            var fileName = string.Concat(DateTime.Now.ToString("MMddyyyy_"), messageId, ".", mediaType);

            var filePath = Path.Combine(AppDelegate.TempFolderPath, fileName);

            var asyncRes = await ApiHelper.MakeAsyncFileDownload($"/api/v1/systems/{systemNumber}/mailboxes/{mailboxNumber}/folders/{folderName}/messages/{messageId}/media/{mediaType}", "application/json", token);
            var errorResponse = CheckErrorResponse(asyncRes);
            if (errorResponse != null)
                return errorResponse;

            var mediaResponse = asyncRes.Result;

            var receivedBytes = 0;  
            var totalBytes = mediaResponse.Length;

            using (var stream = mediaResponse.ReceivedStream)
            using (var targetStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                var buffer = new byte[4096];

                do {
                    if (token.IsCancellationRequested)
                    {
                        if (File.Exists(filePath))
                            File.Delete(filePath);

                        return null;
                    }

                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    targetStream.Write(buffer, 0, bytesRead);

                    if (bytesRead == 0)
                    {
                        await Task.Yield();
                        break;
                    }

                    receivedBytes += bytesRead;
                    if (progressReporter == null) continue;

                    var args = new DownloadBytesProgress(receivedBytes, totalBytes);
                    progressReporter.Report(args);
                } while (true);
            }

            return new GetMediaResponse(filePath);
        }
    }
}