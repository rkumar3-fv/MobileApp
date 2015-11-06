using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class ForgotPasswordService : BaseService, IForgotPasswordService
    {
        public async Task<BaseResponse> ExecuteRequest(string recoveryEMail)
        {
            var asyncRes = await ApiHelper.PasswordReset(recoveryEMail);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            return new ForgotPasswordResponse();
        }
    }
}