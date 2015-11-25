using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class CallReservationService : BaseService, ICallReservationService
    {
        public async Task<BaseResponse> ExecuteRequest(string systemNumber, string callerIdNumber, string presentationNumber, string destinationNumber)
        {
            var asyncRes = await ApiHelper.CreateCallReservation(systemNumber, callerIdNumber, presentationNumber, destinationNumber);
            var errorResponse = CheckErrorResponse(asyncRes);
            if (errorResponse != null)
                return errorResponse;

            return new CallReservationResponse(asyncRes.Result);
        }
    }
}