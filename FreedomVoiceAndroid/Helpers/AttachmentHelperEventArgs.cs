using System;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// Attachment helper event arguments
    /// </summary>
    /// <typeparam name="T">attachment event type</typeparam>
    public class AttachmentHelperEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Request ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Attachment's type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Result
        /// </summary>
        public T Result { get; }

        public AttachmentHelperEventArgs(int id, string type, T result)
        {
            Id = id;
            Type = type;
            Result = result;
        } 
    }
}