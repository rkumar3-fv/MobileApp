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

            if (File.Exists(filePath))
                return new GetMediaResponse(filePath);

            var asyncRes = await ApiHelper.MakeAsyncFileDownload($"/api/v1/systems/{systemNumber}/mailboxes/{mailboxNumber}/folders/{folderName}/messages/{messageId}/media/{mediaType}", "application/json", token);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            var mediaResponse = asyncRes.Result;

            var receivedBytes = 0;  
            var totalBytes = mediaResponse.Length;           

            using (var stream = mediaResponse.ReceivedStream)
            using (var targetStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                if (progressReporter == null)
                {
                    await stream.CopyToAsync(targetStream, 4096, token);
                }
                else
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) != 0)
                    {
                        await targetStream.WriteAsync(buffer, 0, bytesRead, token);
                        progressReporter.Report(new DownloadBytesProgress(receivedBytes, totalBytes));
                    }
                }
            }

            if (token.IsCancellationRequested)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return null;
            }

            return new GetMediaResponse(filePath);
        }
    }
}