using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    public class AttachmentsServiceResultReceiver : AppServiceResultReceiver
    {
        public const string ReceiverTag = "AttachmentsServiceResultReceiver";
        public const string ReceiverDataExtra = "AttachmentsServiceResultReceiverExtra";

        public AttachmentsServiceResultReceiver(Handler handler) : base(handler)
        {}
    }
}