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
                case ErrorCodes.NotFound:
                    return new ErrorResponse(ErrorResponse.ErrorNotFound);
                case ErrorCodes.PaymentRequired:
                    return new ErrorResponse(ErrorResponse.ErrorPaymentRequired);
                case ErrorCodes.Forbidden:
                    return new ErrorResponse(ErrorResponse.Forbidden);
                case ErrorCodes.Unknown:
                    return new ErrorResponse(ErrorResponse.ErrorUnknown);
                default:
                    return null;
            }
        }
    }
}