using Android.Content;
using Android.Support.V4.App;

namespace com.FreedomVoice.MobileApp.Android.Notifications
{
    /// <summary>
    /// Base abstract notification helper
    /// </summary>
    public abstract class BaseNotification
    {
        protected NotificationCompat.Builder AppNotification;
        protected readonly Context AppContext;
        protected readonly NotificationManagerCompat NotificationManager;

        protected BaseNotification(Context context)
        {
            AppContext = context;
            NotificationManager = (NotificationManagerCompat)AppContext.GetSystemService(Context.NotificationService);
            AppNotification = new NotificationCompat.Builder(AppContext);
        }

        /// <summary>
        /// Notification ID
        /// </summary>
        public abstract int NotificationCode();

        /// <summary>
        /// Show notification
        /// </summary>
        public virtual void ShowNotification(string content)
        {
            AppNotification.SetContentText(content);
            NotificationManager.Notify(NotificationCode(), AppNotification.Build());
        }

        /// <summary>
        /// Hide notification
        /// </summary>
        public void HideNotification()
        {
            NotificationManager.Cancel(NotificationCode());
        }
    }
}