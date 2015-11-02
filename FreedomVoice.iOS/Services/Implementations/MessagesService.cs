using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class MessagesService : BaseService, IMessagesService
    {
        private const int PageSize = 30;

        public async Task<BaseResponse> ExecuteRequest(string systemNumber, int mailboxNumber, string folderName)
        {
            var asyncRes = await ApiHelper.GetMesages(systemNumber, mailboxNumber, folderName, PageSize, 1, false);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            return new MessagesResponse(asyncRes.Result);
        }
    }
}