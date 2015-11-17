using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class LoginService : BaseService, ILoginService
    {
        public async Task<BaseResponse> ExecuteRequest(string userName, string password)
        {
            var asyncRes = await ApiHelper.Login(userName, password);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            return new EmptyResponse();
        }
    }
}