namespace FreedomVoice.iOS.Services.Responses
{
    public class ErrorResponse : BaseResponse
    {
        public const int ErrorBadRequest = 1;
        public const int ErrorCancelled = 2;
        public const int ErrorConnection = 3;
        public const int ErrorUnauthorized = 4;
        public const int PaymentRequired = 5;
        public const int ErrorUnknown = 0;

        public ErrorResponse(int errorCode)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Response Error Code
        /// </summary>
        public int ErrorCode { get; }
    }
}