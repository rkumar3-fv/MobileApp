using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class AccountsService : BaseService, IAccountsService
    {
        public async Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.GetSystems();
            var errorResponse = CheckErrorResponse(asyncRes);
            if (errorResponse != null)
                return errorResponse;

            return new AccountsResponse(asyncRes.Result.PhoneNumbers);
        }
    }
}