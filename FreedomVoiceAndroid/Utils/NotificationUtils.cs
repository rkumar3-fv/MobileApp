using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public static class NotificationUtils
    {
        public static NotificationCompat.Builder GetDefaultBuilder(Context context)
        {
            var channelId = context.GetString(Resource.String.DefaultNotificationChannel);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var manager = NotificationManager.FromContext(context);
                manager.CreateNotificationChannel(
                    new NotificationChannel(
                        channelId,
                        context.GetString(Resource.String.ApplicationName),
                        NotificationImportance.Default
                    )
                );
            }
            return new NotificationCompat.Builder(context, channelId)
                .SetChannelId(channelId);
        }

        public static NotificationCompat.Builder GetProgressBuilder(Context context)
        {
            var channelId = context.GetString(Resource.String.DownloadNotificationChannel);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var manager = NotificationManager.FromContext(context);
                manager.CreateNotificationChannel(
                    new NotificationChannel(
                        channelId,
                        context.GetString(Resource.String.DownloadNotificationChannel),
                        NotificationImportance.Low
                    )
                );
            }

            return new NotificationCompat.Builder(context, channelId)
                .SetChannelId(channelId)
                .SetSound(null)
                .SetPriority((int) NotificationPriority.Low);
        }
        
        
    }
}