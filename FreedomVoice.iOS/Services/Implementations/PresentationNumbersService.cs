using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class PresentationNumbersService : BaseService, IPresentationNumbersService
    {
        private string _systemNumber;

        public void SetSystemNumber(string systemNumber)
        {
            _systemNumber = systemNumber;
        }

        public async override Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.GetPresentationPhoneNumbers(_systemNumber);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            return new PresentationNumbersResponse(asyncRes.Result.PhoneNumbers);
        }
    }
}