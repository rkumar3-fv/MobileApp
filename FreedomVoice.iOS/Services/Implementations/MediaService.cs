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
        private const int BufferSize = 4096;

        public async Task<BaseResponse> ExecuteRequest(IProgress<DownloadBytesProgress> progressReporter, string systemNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType, string filePath, CancellationToken token)
        {
            var asyncResult = await ApiHelper.MakeAsyncFileDownload($"/api/v1/systems/{systemNumber}/mailboxes/{mailboxNumber}/folders/{folderName}/messages/{messageId}/media/{mediaType}", token);
            var errorResponse = CheckErrorResponse(asyncResult);
            if (errorResponse != null)
                return errorResponse;

            var totalBytes = asyncResult.Result.Length;
            var receivedBytes = 0;

            try
            {
                using (var receivedStream = asyncResult.Result.ReceivedStream)
                using (var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                {
                    var buffer = new byte[BufferSize];
                    int bytesRead;

                    while ((bytesRead = await receivedStream.ReadAsync(buffer, 0, BufferSize, token)) > 0)
                    {
                        token.ThrowIfCancellationRequested();

                        fs.Write(buffer, 0, bytesRead);

                        receivedBytes += bytesRead;

                        progressReporter?.Report(new DownloadBytesProgress(receivedBytes, totalBytes));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return new ErrorResponse(ErrorResponse.ErrorCancelled);
            }
            catch (InvalidOperationException)
            {
                return new ErrorResponse(ErrorResponse.ErrorConnection);
            }
            catch (Exception)
            {
                return new ErrorResponse(ErrorResponse.ErrorBadRequest);
            }

            return new EmptyResponse();
        }
    }
}