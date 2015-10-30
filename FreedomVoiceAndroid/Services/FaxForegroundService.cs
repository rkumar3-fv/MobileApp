using Android.App;
using Android.Content;
using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    /// <summary>
    /// Faxes uploading service
    /// Used in foreground mode
    /// </summary>
    [Service(Exported = false)]
    public class FaxForegroundService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}