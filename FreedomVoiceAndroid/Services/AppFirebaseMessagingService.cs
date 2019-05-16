using System;
using System.Drawing;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Firebase.Messaging;
using FreedomVoice.Entities.PushContract;
using Newtonsoft.Json;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    [Service]
    [IntentFilter(new[] {"com.google.firebase.MESSAGING_EVENT"})]
    public class AppFirebaseMessagingService : Firebase.Messaging.FirebaseMessagingService
    {
        private const string DataKey = "data";

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            if (message.Data == null || !message.Data.ContainsKey(DataKey)) return;

            try
            {
                PushNotification pushNotification =
                    JsonConvert.DeserializeObject<PushNotification>(message.Data[DataKey]);

                ShowNotification(pushNotification);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ShowNotification(PushNotification push)
        {
            var manager = NotificationManagerCompat.From(this);
            var channelId = GetString(Resource.String.DefaultNotificationChannel);
            var launchIntent = PackageManager.GetLaunchIntentForPackage(ApplicationContext.PackageName);
            var pLaunchIntent = PendingIntent.GetActivity(this, 0, launchIntent, PendingIntentFlags.UpdateCurrent);
            var notification = new NotificationCompat.Builder(this, channelId)
                .SetChannelId(channelId)
                .SetSmallIcon(Resource.Drawable.ic_sms)
                .SetColor(ContextCompat.GetColor(this, Resource.Color.colorActivatedControls))
                .SetAutoCancel(true)
                .SetSound(Settings.System.DefaultNotificationUri)
                .SetContentIntent(pLaunchIntent)
                .SetContentTitle(push.Aps.Alert.Title)
                .SetSubText(push.Aps.Alert.Subtitle)
                .SetContentText(push.Aps.Alert.Body);


            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.O)
            {
                if (GetSystemService(Context.NotificationService) is NotificationManager newManager)
                {
                    var notificationChannel = newManager.GetNotificationChannel(channelId) ??
                                              new NotificationChannel(
                                                  channelId,
                                                  GetString(Resource.String.ApplicationName),
                                                  NotificationImportance.Default
                                              );
                    newManager.CreateNotificationChannel(notificationChannel);
                }
            }

            manager.Notify(1, notification.Build());
        }
    }
}