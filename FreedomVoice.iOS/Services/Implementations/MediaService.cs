using System.Threading;
using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class MediaService : BaseService, IMediaService
    {
        public async Task<BaseResponse> ExecuteRequest(string systemNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType, CancellationToken token)
        {
            var asyncRes = await ApiHelper.GetMedia(systemNumber, mailboxNumber, folderName, messageId, mediaType, token);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            return new MediaResponse(asyncRes.Result, mediaType);
        }
    }
}