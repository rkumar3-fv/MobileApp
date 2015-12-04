using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities;
using FreedomVoice.iOS.Services.Responses;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class MessagesService : BaseService, IMessagesService
    {
        private const int PageSize = 30;

        public async Task<BaseResponse> ExecuteRequest(string systemNumber, int mailboxNumber, string folderName, List<Contact> contactList)
        {            
            int messageCount;
            var pageNumber = 1;

            var result = new List<Message>();
            do
            {
                var asyncRes = await ApiHelper.GetMesages(systemNumber, mailboxNumber, folderName, PageSize, pageNumber, false);
                var errorResponse = CheckErrorResponse(asyncRes);
                if (errorResponse != null)
                    return errorResponse;

                messageCount = asyncRes.Result.Count;
                result.AddRange(asyncRes.Result);
                pageNumber++;
            } while (messageCount == PageSize);

            return new MessagesResponse(result, contactList);
        }
    }
}