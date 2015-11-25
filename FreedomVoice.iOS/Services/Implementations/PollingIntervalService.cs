using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class PollingIntervalService : BaseService, IPollingIntervalService
    {
        public async Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.GetPollingInterval();
            var errorResponse = CheckErrorResponse(asyncRes);
            if (errorResponse != null)
                return errorResponse;

            return new PollingIntervalResponse(asyncRes.Result);
        }
    }
}