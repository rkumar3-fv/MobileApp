using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class FoldersService : BaseService, IFoldersService
    {
        public async Task<BaseResponse> ExecuteRequest(string systemNumber, int mailboxNumber)
        {
            var asyncRes = await ApiHelper.GetFoldersWithCount(systemNumber, mailboxNumber);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            return new FoldersWithCountResponse(asyncRes.Result);
        }
    }
}