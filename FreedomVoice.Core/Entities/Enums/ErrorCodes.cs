namespace FreedomVoice.Core.Entities.Enums
{
    public enum ErrorCodes
    {
        Ok,
        Unauthorized,
        BadRequest,
        ConnectionLost,
        Cancelled,
        Unknown,
        NotFound,
        Forbidden, 
        PaymentRequired,
        InternalServerError,
        RequestTimeout,
        GatewayTimeout
    }
}