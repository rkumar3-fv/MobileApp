using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using FreedomVoice.Core.Entities.Enums;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Abstract request action
    /// <see href="https://api.freedomvoice.com/Help">FreedomVoice REST API</see>
    /// </summary>
    [Preserve(AllMembers = true)]
    public abstract class BaseRequest : BaseAction, IEquatable<BaseRequest>
    {
        public long Id { get; }

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
        /// <param name="json">json of response</param>
        /// <returns>null or ErrorResponse</returns>
        protected ErrorResponse CheckErrorResponse(long requestId, ErrorCodes code, string json)
        {
            switch (code)
            {
                case ErrorCodes.Ok:
                    return null;
                case ErrorCodes.BadRequest:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorBadRequest, json);
                case ErrorCodes.Cancelled:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorCancelled, json);
                case ErrorCodes.ConnectionLost:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorConnection, json);
                case ErrorCodes.Unauthorized:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorUnauthorized, json);
                case ErrorCodes.NotFound:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorNotFound, json);
                case ErrorCodes.PaymentRequired:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorNotPaid, json);
                case ErrorCodes.Forbidden:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorForbidden, json);
                case ErrorCodes.InternalServerError:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorInternal, json);
                default:
                    return new ErrorResponse(requestId, ErrorResponse.ErrorUnknown, json);
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

        public bool Equals(BaseRequest other)
        {
            return !ReferenceEquals(null, other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BaseRequest) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ Id.GetHashCode();
            }
        }
    }
}