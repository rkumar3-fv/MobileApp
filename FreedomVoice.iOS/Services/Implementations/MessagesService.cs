using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;
using System.Collections.Generic;
using FreedomVoice.Core.Entities;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class MessagesService : BaseService, IMessagesService
    {
        private const int PageSize = 30;

        public async Task<BaseResponse> ExecuteRequest(string systemNumber, int mailboxNumber, string folderName, int messageCount)
        {            
            int pagesTotal = (messageCount + PageSize - 1) / PageSize;            
            List<Message> result = new List<Message>();
            for (int i = 0; i < pagesTotal; i++)
            {
                var asyncRes = await ApiHelper.GetMesages(systemNumber, mailboxNumber, folderName, PageSize, i + 1, false);
                var errorResponse = CheckErrorResponse(asyncRes.Code);
                if (errorResponse != null)
                    return errorResponse;

                result.AddRange(asyncRes.Result);
            }

            return new MessagesResponse(result);
        }
    }
}