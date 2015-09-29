using Android.App;
using Android.Content;
using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    /// <summary>
    /// Background server-client communication service
    /// </summary>
    [Service (Exported = false)]
    public class ComService : Service
    {
        public const string RequestTag = "ComServiceRequest";
        public const string RequestIdTag = "ComServiceRequestId";

        private ComServiceResultReceiver _receiver;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (_receiver != null)
                _receiver = intent.GetParcelableExtra(ComServiceResultReceiver.ReceiverTag) as ComServiceResultReceiver;
            return StartCommandResult.NotSticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _receiver = null;
        }
    }
}