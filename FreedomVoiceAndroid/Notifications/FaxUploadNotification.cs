using Android.App;
using Android.Content;

namespace com.FreedomVoice.MobileApp.Android.Notifications
{
    /// <summary>
    /// Single instance fax notification
    /// </summary>
    public class FaxUploadNotification: BaseNotification
    {
        private static volatile FaxUploadNotification _instance;
        private static readonly object Locker = new object();

        /// <summary>
        /// Get fax uploading notification instance
        /// </summary>
        public static FaxUploadNotification Instance(Context context)
        {
            if (_instance != null) return _instance;
            lock (Locker)
            {
                if (_instance == null)
                    _instance = new FaxUploadNotification(context);
            }
            return _instance;
        }

        private FaxUploadNotification(Context context) : base(context)
        {}

        /// <summary>
        /// Show new fax downloading notification
        /// </summary>
        /// <param name="title">Message title</param>
        public override void ShowNotification(string title)
        {
            AppNotification.SetAutoCancel(false);
            AppNotification.SetOngoing(true);
            AppNotification.SetProgress(100, 0, false);
            AppNotification.SetContentText(AppContext.GetString(Resource.String.Notif_downloadProgress));
            //AppNotification.SetSmallIcon(Resource.Drawable.);
            //AppNotification.SetLargeIcon(BitmapFactory.DecodeResource(Resource.Drawable.));
            base.ShowNotification(title);
        }

        /// <summary>
        /// Update fax downloading progress
        /// </summary>
        /// <param name="progress">progress value in percentages</param>
        public void UpdateProgress(int progress)
        {
            if (progress > 100) return;
            AppNotification.SetProgress(100, progress, false);
            NotificationManager.Notify(NotificationCode(), AppNotification.Build());
        }

        /// <summary>
        /// Fax loading finished notification
        /// </summary>
        /// <param name="intent">intent for fax opening</param>
        public void FinishLoading(Intent intent)
        {
            var stackBuilder = TaskStackBuilder.Create(AppContext);
            stackBuilder.AddNextIntent(intent);
            var resultPendingIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);

            AppNotification.SetOngoing(false);
            AppNotification.SetContentText(AppContext.GetString(Resource.String.Notif_downloadComplete));
            //AppNotification.SetSmallIcon(Resource.Drawable.);
            //AppNotification.SetLargeIcon(BitmapFactory.DecodeResource(Resource.Drawable.));
            AppNotification.SetProgress(0, 0, false);
            AppNotification.SetAutoCancel(true);
            AppNotification.SetContentIntent(resultPendingIntent);
            NotificationManager.Notify(NotificationCode(), AppNotification.Build());
        }

        /// <summary>
        /// Notification ID
        /// </summary>
        public override int NotificationCode()
        {
            return 100;
        }
    }
}