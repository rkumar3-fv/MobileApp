using System;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// ActionHelper result data callback
    /// </summary>
    public class ActionsHelperEventArgs : EventArgs, IEquatable<ActionsHelperEventArgs>
    {
        public const int AuthLoginError = 11;
        public const int AuthPasswdError = 12;
        public const int RestoreError = 21;
        public const int RestoreWrongEmail = 22;
        public const int RestoreOk = 23;
        public const int MsgExtensionsUpdated = 31;
        public const int MsgFoldersUpdated = 32;
        public const int MsgMessagesUpdated = 33;

        public ActionsHelperEventArgs(long requestId, int code)
        {
            RequestId = requestId;
            Code = code;
        }

        /// <summary>
        /// Request ID
        /// </summary>
        public long RequestId { get; }

        /// <summary>
        /// Response code
        /// </summary>
        public int Code { get; }
        
        /// <summary>
        /// Response equality comparer
        /// </summary>
        /// <param name="other">Another code</param>
        /// <returns>equals or not</returns>
        public bool Equals(ActionsHelperEventArgs other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RequestId == other.RequestId && Code == other.Code;
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
                return (RequestId.GetHashCode()*397) ^ Code;
            }
        }
    }
}