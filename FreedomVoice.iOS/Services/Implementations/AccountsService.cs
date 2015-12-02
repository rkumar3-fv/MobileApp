using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class AccountsService : BaseService, IAccountsService
    {
        public async Task<BaseResponse> ExecuteRequest(bool noCache)
        {
            var asyncRes = await ApiHelper.GetSystems(noCache);
            var errorResponse = CheckErrorResponse(asyncRes);
            if (errorResponse != null)
                return errorResponse;

            return new AccountsResponse(asyncRes.Result.PhoneNumbers);
        }
    }
}