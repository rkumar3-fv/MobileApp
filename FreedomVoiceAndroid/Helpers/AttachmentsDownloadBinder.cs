using Android.OS;
using com.FreedomVoice.MobileApp.Android.Services;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    public class AttachmentsDownloadBinder : Binder
    {
        public AttachmentsDownloadBinder(AttachmentsDownloadService service)
        {
            Service = service;
        }

        public AttachmentsDownloadService Service { get; }
    }
}