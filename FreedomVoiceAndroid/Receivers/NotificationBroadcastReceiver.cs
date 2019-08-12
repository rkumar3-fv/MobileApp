using System.IO;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Helpers;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Receivers
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] {"com.freedomvoice.android.notif"})]
    public class NotificationBroadcastReceiver : BroadcastReceiver
    {
        public const string ExtraPdfPath = "ExtraPdfPath";
        public const string ExtraNotificationClick = "ExtraNotificationClick";

        public override void OnReceive(Context context, Intent intent)
        {
            var application = App.GetApplication(context);
            if (intent.HasExtra(ExtraPdfPath))
            {
                ProcessPdf(context, intent);
            }
            else if (intent.HasExtra(BaseActivity.NavigationRedirectActivityName))
            {
                var activityName = intent.GetStringExtra(BaseActivity.NavigationRedirectActivityName);
                var payload = intent.GetBundleExtra(BaseActivity.NavigatePayloadBundle);
                var redirect = new Intent(context, Class.ForName(activityName));
                if (application.ApplicationHelper.ActionsHelper.IsLoggedIn)
                {
                    if (application.IsColdStart)
                    {
                        redirect.PutExtras(payload);
                        application.ApplicationHelper.NavigationRedirectHelper
                            .AddRedirect(new NavigationRedirectHelper.ActivityRedirect(redirect));

                        StartDefaultLauncher(application);
                    }
                    else
                    {
                        redirect.PutExtras(payload);
                        redirect.SetFlags(ActivityFlags.NewTask | ActivityFlags.ResetTaskIfNeeded);
                        application.StartActivity(redirect);
                    }
                }
                else
                {
                    StartDefaultLauncher(application);
                }
            }
        }

        private static void StartDefaultLauncher(Context context)
        {
            var launchIntentForPackage = context.PackageManager
                .GetLaunchIntentForPackage(context.PackageName)
                .SetPackage(null)
                .SetFlags(ActivityFlags.NewTask | ActivityFlags.ResetTaskIfNeeded);
            context.StartActivity(launchIntentForPackage);
        }

        private void ProcessPdf(Context context, Intent intent)
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