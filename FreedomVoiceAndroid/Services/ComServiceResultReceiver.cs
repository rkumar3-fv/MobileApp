using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    public class ComServiceResultReceiver : AppServiceResultReceiver
    {
        public const string ReceiverTag = "ComServiceResultReceiver";
        public const string ReceiverDataExtra = "ComServiceResultReceiverExtra";

        public ComServiceResultReceiver(Handler handler) : base(handler)
        {}
    }
}