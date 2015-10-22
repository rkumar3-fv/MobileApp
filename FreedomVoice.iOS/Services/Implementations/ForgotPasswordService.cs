using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class ForgotPasswordService : BaseService, IForgotPasswordService
    {
        private string _recoveryEMail;

        public void SetRecoveryEmail(string email)
        {
            _recoveryEMail = email;
        }

        public async override Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.PasswordReset(_recoveryEMail);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            return new ForgotPasswordResponse();
        }
    }
}