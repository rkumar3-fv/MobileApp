using System;
using System.Drawing;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using com.FreedomVoice.MobileApp.Android.Activities;
using Firebase.Messaging;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.Entities.PushContract;
using Java.Lang;
using Newtonsoft.Json;
using Exception = System.Exception;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    [Service]
    [IntentFilter(new[] {"com.google.firebase.MESSAGING_EVENT"})]
    public class AppFirebaseMessagingService : Firebase.Messaging.FirebaseMessagingService
    {
        private IContactNameProvider _contactNameProvider;
        private const string DataKey = "data";

        public override void OnCreate()
        {
            base.OnCreate();
            _contactNameProvider = ServiceContainer.Resolve<IContactNameProvider>();
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            if (message.Data == null || !message.Data.ContainsKey(DataKey)) return;

            try
            {
                PushNotification pushNotification =
                    JsonConvert.DeserializeObject<PushNotification>(message.Data[DataKey]);

                using (var h = new Handler(Looper.MainLooper))
                {
                    h.Post(() =>
                    {
                        var name = _contactNameProvider.GetName(pushNotification.Data.Message.From.PhoneNumber);
                        pushNotification.Aps.Alert.Title = name;

                        ShowConversationMessagePush(pushNotification);
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ShowConversationMessagePush(PushNotification push)
        {
            var manager = NotificationManagerCompat.From(this);
            var channelId = GetString(Resource.String.DefaultNotificationChannel);

            PendingIntent pLaunchIntent;
            if (App.GetApplication(this).ApplicationHelper.ActionsHelper.IsLoggedIn)
            {
                var launchIntent =
                    ChatActivity.OpenChat(this, push.Data.Message.Id, push.Data.Message.From.PhoneNumber);
                pLaunchIntent = PendingIntent.GetActivity(this, 0, launchIntent, PendingIntentFlags.UpdateCurrent);
            }
            else
            {
                pLaunchIntent = TaskStackBuilder.Create(this)
                    .AddParentStack(Class.FromType(typeof(ContentActivity)))
                    .AddNextIntent(new Intent(this, typeof(ContentActivity)))
                    .AddNextIntent( // TODO replace push.Data.Message.Id to push.Data.Message.ConversationID !!
                        ChatActivity.OpenChat(this, push.Data.Message.Id, push.Data.Message.From.PhoneNumber)
                    ).GetPendingIntent(0, (int) PendingIntentFlags.UpdateCurrent);
            }


            var notification = new NotificationCompat.Builder(this, channelId)
                .SetChannelId(channelId)
                .SetSmallIcon(Resource.Drawable.ic_default_notification)
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