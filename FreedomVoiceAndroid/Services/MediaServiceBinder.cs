using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    /// <summary>
    /// Voicemail and records media service binding implementation
    /// </summary>
    public class MediaServiceBinder : Binder
    {
        /// <summary>
        /// Media service
        /// </summary>
        public MediaService AppMediaService { get; }

        public MediaServiceBinder(MediaService service)
        {
            AppMediaService = service;
        }
    }
}