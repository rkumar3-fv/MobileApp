using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class MessagesService : BaseService, IMessagesService
    {
        private const int PageSize = 30;

        private string _systemNumber;
        private int _mailboxNumber;
        private string _folderName;

        public void SetParameters(string systemNumber, int mailboxNumber, string folderName)
        {
            _systemNumber = systemNumber;
            _mailboxNumber = mailboxNumber;
            _folderName = folderName;
        }

        public async override Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.GetMesages(_systemNumber, _mailboxNumber, _folderName, PageSize, 1, true);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            return new MessagesResponse(asyncRes.Result);
        }
    }
}