using System.IO;
using Android.Content;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Receivers
{
    [BroadcastReceiver(Exported= false)]
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

            var application = App.GetApplication(context);
            var openPdfFileIntent = FileUtils.OpenPdfFileIntent(application, path);
            JavaSystem.Gc();
            try
            {
                application.StartActivity(openPdfFileIntent);
            }
            catch (ActivityNotFoundException)
            {
                Toast.MakeText(context, Resource.String.Snack_pdfError, ToastLength.Long).Show();
            }
        }
    }
}