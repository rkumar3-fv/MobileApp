using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class LoginService : BaseService, ILoginService
    {
        private string _userName;
        private string _password;

        public void SetCredentials(string userName, string password)
        {
            _userName = userName;
            _password = password;
        }

        public async override Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.Login(_userName, _password);
            var errorResponse = CheckErrorResponse(asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;

            return new LoginResponse();
        }
    }
}