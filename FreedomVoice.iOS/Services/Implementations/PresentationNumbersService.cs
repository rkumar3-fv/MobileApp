using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class PresentationNumbersService : BaseService, IPresentationNumbersService
    {
        public async Task<BaseResponse> ExecuteRequest(string systemNumber)
        {
            var asyncRes = await ApiHelper.GetPresentationPhoneNumbers(systemNumber);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            return new PresentationNumbersResponse(asyncRes.Result.PhoneNumbers);
        }
    }
}