using System;
using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// ActionHelper result data callback
    /// </summary>
    public class ActionsHelperEventArgs : EventArgs, IEquatable<ActionsHelperEventArgs>
    {
        public ActionsHelperEventArgs(long requestId, Bundle dataBundle)
        {
            RequestId = requestId;
            DataBundle = dataBundle;
        }

        /// <summary>
        /// Request ID
        /// </summary>
        public long RequestId { get; }

        /// <summary>
        /// Bundled response data
        /// </summary>
        public Bundle DataBundle { get; }

        /// <summary>
        /// Response equality comparer
        /// </summary>
        /// <param name="other">Another response callback</param>
        /// <returns>equals or not</returns>
        public bool Equals(ActionsHelperEventArgs other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RequestId == other.RequestId && Equals(DataBundle, other.DataBundle);
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
                return (RequestId.GetHashCode()*397) ^ (DataBundle?.GetHashCode() ?? 0);
            }
        }
    }
}