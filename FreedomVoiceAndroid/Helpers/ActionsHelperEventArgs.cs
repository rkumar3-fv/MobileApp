using System;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// ActionHelper result data callback
    /// </summary>
    public class ActionsHelperEventArgs : EventArgs, IEquatable<ActionsHelperEventArgs>
    {
        public ActionsHelperEventArgs(long requestId, BaseResponse response)
        {
            RequestId = requestId;
            ResponseData = response;
        }

        /// <summary>
        /// Request ID
        /// </summary>
        public long RequestId { get; }

        /// <summary>
        /// Response data
        /// </summary>
        public BaseResponse ResponseData { get; }

        /// <summary>
        /// Response equality comparer
        /// </summary>
        /// <param name="other">Another response callback</param>
        /// <returns>equals or not</returns>
        public bool Equals(ActionsHelperEventArgs other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RequestId == other.RequestId && Equals(ResponseData, other.ResponseData);
        }

        /// <summary>
        /// Response equality comparer
        /// </summary>
        /// <param name="obj">Another object</param>
        /// <returns>equals or not</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ActionsHelperEventArgs) obj);
        }

        /// <summary>
        /// Response callback hashcode
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return (RequestId.GetHashCode()*397) ^ (ResponseData?.GetHashCode() ?? 0);
            }
        }
    }
}