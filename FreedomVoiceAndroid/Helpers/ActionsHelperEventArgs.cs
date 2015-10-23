using System;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// ActionHelper result data callback
    /// </summary>
    public class ActionsHelperEventArgs : EventArgs, IEquatable<ActionsHelperEventArgs>
    {
        public const int ConnectionLostError = 1;       //Connection lost
        public const int AuthLoginError = 11;           //BadRequest
        public const int AuthPasswdError = 12;          //Unauthorized
        public const int RestoreError = 21;             //BadRequest
        public const int RestoreWrongEmail = 22;        //NotFound
        public const int RestoreOk = 23;                //OK
        public const int MsgUpdated = 31;
        public const int MsgFoldersUpdated = 32;
        public const int MsgMessagesUpdated = 33;
        public const int CallReservationOk = 41;        //CallReservation - phone
        public const int CallReservationFail = 42;      //CallReservation - BadRequest or empty
        public const int ClearRecents = 43;             //ClearRecents

        public ActionsHelperEventArgs(long requestId, int[] codes)
        {
            RequestId = requestId;
            Codes = codes;
        }

        /// <summary>
        /// Request ID
        /// </summary>
        public long RequestId { get; }

        /// <summary>
        /// Response codes
        /// </summary>
        public int[] Codes { get; }

        public bool Equals(ActionsHelperEventArgs other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RequestId == other.RequestId && Equals(Codes, other.Codes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ActionsHelperEventArgs) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (RequestId.GetHashCode()*397) ^ (Codes?.GetHashCode() ?? 0);
            }
        }
    }
}