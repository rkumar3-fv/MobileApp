using Android.App;
using Android.Content;
using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    /// <summary>
    /// Media playback service
    /// </summary>
    [Service(Exported = false)]
    public class MediaService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}