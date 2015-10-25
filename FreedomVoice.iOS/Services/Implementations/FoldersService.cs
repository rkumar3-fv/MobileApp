using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class FoldersService : BaseService, IFoldersService
    {
        private string _systemNumber;
        private int _mailboxNumber;

        public void SetParameters(string systemNumber, int mailboxNumber)
        {
            _systemNumber = systemNumber;
            _mailboxNumber = mailboxNumber;
        }

        public async override Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.GetFoldersWithCount(_systemNumber, _mailboxNumber);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            return new FoldersWithCountResponse(asyncRes.Result);
        }
    }
}