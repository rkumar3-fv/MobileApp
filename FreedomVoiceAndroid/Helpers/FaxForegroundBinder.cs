using Android.OS;
using com.FreedomVoice.MobileApp.Android.Services;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    public class FaxForegroundBinder : Binder
    {
        public FaxForegroundBinder(FaxForegroundService service)
        {
            Service = service;
        }

        public FaxForegroundService Service { get; }
    }
}