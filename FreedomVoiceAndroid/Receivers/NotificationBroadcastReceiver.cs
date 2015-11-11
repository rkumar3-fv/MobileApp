using System.IO;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Widget;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Receivers
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "com.freedomvoice.android.notif" })]
    public class NotificationBroadcastReceiver : BroadcastReceiver
    {
        public const string ExtraPdfPath = "ExtraPdfPath";

        public override void OnReceive(Context context, Intent intent)
        {
            var path = intent.GetStringExtra(ExtraPdfPath);
            if (path == null) return;
            if (!File.Exists(path)) return;
            var pdfIntent = new Intent(Intent.ActionView);
            var file = new Java.IO.File(path);
            file.SetReadable(true);
            pdfIntent.SetDataAndType(Uri.FromFile(file), "application/pdf");
            pdfIntent.SetFlags(ActivityFlags.NewTask);
            pdfIntent.AddFlags(ActivityFlags.NoHistory);
            JavaSystem.Gc();
            try
            {
                App.GetApplication(context).StartActivity(pdfIntent);
            }
            catch (ActivityNotFoundException)
            {
                Toast.MakeText(context, Resource.String.Snack_pdfError, ToastLength.Long).Show();
            }
        }
    }
}