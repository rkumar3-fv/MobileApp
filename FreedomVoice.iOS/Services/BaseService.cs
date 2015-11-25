using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public abstract class BaseService
    {
        protected static ErrorResponse CheckErrorResponse<T>(BaseResult<T> baseResult, bool checkOnNullResult = true)
        {
            if (baseResult.Code == ErrorCodes.Ok && checkOnNullResult && baseResult.Result == null)
                return new ErrorResponse(ErrorResponse.ErrorInternal);

            switch (baseResult.Code)
            {
                case ErrorCodes.Ok:
                    return null;
                case ErrorCodes.BadRequest:
                    return new ErrorResponse(ErrorResponse.ErrorBadRequest);
                case ErrorCodes.Cancelled:
                    return new ErrorResponse(ErrorResponse.ErrorCancelled);
                case ErrorCodes.ConnectionLost:
                    return new ErrorResponse(ErrorResponse.ErrorConnection);
                case ErrorCodes.Unauthorized:
                    return new ErrorResponse(ErrorResponse.ErrorUnauthorized);
                case ErrorCodes.NotFound:
                    return new ErrorResponse(ErrorResponse.ErrorNotFound);
                case ErrorCodes.PaymentRequired:
                    return new ErrorResponse(ErrorResponse.ErrorPaymentRequired);
                case ErrorCodes.Forbidden:
                    return new ErrorResponse(ErrorResponse.Forbidden);
                case ErrorCodes.InternalServerError:
                    return new ErrorResponse(ErrorResponse.ErrorInternal);
                default:
                    return new ErrorResponse(ErrorResponse.ErrorUnknown);
            }
        }
    }
}