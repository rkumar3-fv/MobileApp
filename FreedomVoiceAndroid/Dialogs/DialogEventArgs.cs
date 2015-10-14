using System;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    /// <summary>
    /// Dialog result event args
    /// </summary>
    public class DialogEventArgs : EventArgs, IEquatable<DialogEventArgs>
    {
        /// <summary>
        /// Dialog result
        /// </summary>
        public DialogResult Result { get; }

        public DialogEventArgs(DialogResult result)
        {
            Result = result;
        }

        public bool Equals(DialogEventArgs other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Result == other.Result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DialogEventArgs) obj);
        }

        public override int GetHashCode()
        {
            return (int) Result;
        }
    }
}