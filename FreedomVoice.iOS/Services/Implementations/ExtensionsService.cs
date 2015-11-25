using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class ExtensionsService : BaseService, IExtensionsService
    {
        public async Task<BaseResponse> ExecuteRequest(string systemNumber)
        {
            var asyncRes = await ApiHelper.GetMailboxesWithCounts(systemNumber);
            var errorResponse = CheckErrorResponse(asyncRes);
            if (errorResponse != null)
                return errorResponse;

            return new ExtensionsWithCountResponse(asyncRes.Result);
        }
    }
}