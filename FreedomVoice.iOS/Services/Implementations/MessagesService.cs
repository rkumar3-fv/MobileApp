using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class MessagesService : BaseService, IMessagesService
    {
        private const int PageSize = 30;

        public async Task<BaseResponse> ExecuteRequest(string systemNumber, int mailboxNumber, string folderName, int messageCount)
        {            
            int pagesTotal = (messageCount + PageSize - 1) / PageSize;  
                      
            var result = new List<Message>();

            for (int i = 0; i < pagesTotal; i++)
            {
                var asyncRes = await ApiHelper.GetMesages(systemNumber, mailboxNumber, folderName, PageSize, i + 1, false);
                var errorResponse = CheckErrorResponse(asyncRes.Code);
                if (errorResponse != null)
                    return errorResponse;

                result.AddRange(asyncRes.Result);
            }

            var contactList = await AppDelegate.GetContactsListAsync();

            return new MessagesResponse(result, contactList);
        }
    }
}