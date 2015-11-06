using System;
using Android.Content;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// Action helper response wiht intent
    /// </summary>
    public class ActionsHelperIntentArgs : EventArgs, IEquatable<ActionsHelperIntentArgs>
    {
        public ActionsHelperIntentArgs(long requestId, Intent intent)
        {
            RequestId = requestId;
            IntentData = intent;
        }

        /// <summary>
        /// Request ID
        /// </summary>
        public long RequestId { get; }

        /// <summary>
        /// Intent
        /// </summary>
        public Intent IntentData { get; }

        public bool Equals(ActionsHelperIntentArgs other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RequestId == other.RequestId && Equals(IntentData, other.IntentData);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ActionsHelperIntentArgs) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (RequestId.GetHashCode()*397) ^ (IntentData?.GetHashCode() ?? 0);
            }
        }
    }
}