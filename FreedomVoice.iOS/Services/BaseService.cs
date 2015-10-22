using System.Threading.Tasks;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public abstract class BaseService
    {
        protected static ErrorResponse CheckErrorResponse(ErrorCodes errorCode)
        {
            switch (errorCode)
            {
                case ErrorCodes.BadRequest:
                    return new ErrorResponse(ErrorResponse.ErrorBadRequest);
                case ErrorCodes.Cancelled:
                    return new ErrorResponse(ErrorResponse.ErrorCancelled);
                case ErrorCodes.ConnectionLost:
                    return new ErrorResponse(ErrorResponse.ErrorConnection);
                case ErrorCodes.Unauthorized:
                    return new ErrorResponse(ErrorResponse.ErrorUnauthorized);
                case ErrorCodes.PaymentRequired:
                    return new ErrorResponse(ErrorResponse.PaymentRequired);
                case ErrorCodes.Unknown:
                    return new ErrorResponse(ErrorResponse.ErrorUnknown);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Execute request
        /// </summary>
        /// <returns>Request Successfull Response or Error Response</returns>
        public abstract Task<BaseResponse> ExecuteRequest();
    }
}