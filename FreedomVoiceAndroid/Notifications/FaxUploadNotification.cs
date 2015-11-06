using Android.App;
using Android.Content;
using Android.Graphics;

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
        /// <param name="content">Message content</param>
        public override void ShowNotification(string content)
        {
            AppNotification.SetAutoCancel(false);
            AppNotification.SetOngoing(true);
            AppNotification.SetProgress(100, 100, true);
            AppNotification.SetCategory(Notification.CategoryProgress);
            AppNotification.SetContentTitle(AppContext.GetString(Resource.String.Notif_fax_progress));
            AppNotification.SetSmallIcon(Resource.Drawable.ic_notification_download);
            base.ShowNotification(content);
        }

        /// <summary>
        /// Update fax fail progress
        /// </summary>
        public void FailLoading()
        {
            AppNotification.SetOngoing(false);
            AppNotification.SetProgress(0, 0, false);
            AppNotification.SetAutoCancel(true);
            AppNotification.SetContentTitle(AppContext.GetString(Resource.String.Notif_fax_fail));
            AppNotification.SetSmallIcon(Resource.Drawable.ic_action_close);
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
            AppNotification.SetContentTitle(AppContext.GetString(Resource.String.Notif_fax_success));
            AppNotification.SetCategory(Notification.CategoryTransport);
            AppNotification.SetSmallIcon(Resource.Drawable.ic_notification_fax);
            AppNotification.SetOngoing(false);
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