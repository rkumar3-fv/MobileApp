using System.Threading.Tasks;
using Android.OS;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using FreedomVoice.Core.Entities.Enums;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Abstract request action
    /// <see href="https://webservices.freedomvoice.com/FreedomAPI/FreedomAPI.asmx">FreedomAPI</see>
    /// </summary>
    public abstract class BaseRequest : BaseAction
    {
        protected readonly long Id;

        protected BaseRequest(long id)
        {
            Id = id;
        }

        protected BaseRequest(Parcel parcel)
        {
            Id = parcel.ReadLong();
        }

        /// <summary>
        /// Check response for errors
        /// </summary>
        /// <param name="requestId">request ID</param>
        /// <param name="code">response code</param>
        /// <returns>null or ErrorResponse</returns>
        protected ErrorResponse CheckErrorResponse(long requestId, ErrorCodes code)
        {
            switch (code)
            {
                case ErrorCodes.BadRequest:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorBadRequest);
                case ErrorCodes.Cancelled:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorCancelled);
                case ErrorCodes.ConnectionLost:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorConnection);
                case ErrorCodes.Unauthorized:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorUnauthorized);
                case ErrorCodes.Unknown:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorUnknown);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Execute request
        /// </summary>
        /// <returns>Response for request or ErrorResponse</returns>
        public abstract Task<BaseResponse> ExecuteRequest();

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteLong(Id);
        }
    }
}