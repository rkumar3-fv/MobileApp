using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Activities;
using Firebase.Messaging;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.Entities.Enums;
using FreedomVoice.Entities.Request;
using FreedomVoice.Entities.Response;
using Java.Lang;
using Microsoft.EntityFrameworkCore.Internal;
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
                var pushMessageRequest =
                    JsonConvert.DeserializeObject<PushResponse<object>>(message.Data[DataKey]);
                var pushType = pushMessageRequest.PushType;

                switch (pushType)
                {
                    case PushType.NewMessage:
                        var newMessageRequest = JsonConvert.DeserializeObject<PushResponse<Conversation>>(message.Data[DataKey]);
                        ProcessNewMessage(newMessageRequest.Data);
                        NotificationMessageService.Instance().ReceivedNotification(pushType, newMessageRequest.Data);
                        break;
                    case PushType.StatusChanged:
                        var pushChangeRequest = JsonConvert.DeserializeObject<PushResponse<Conversation>>(message.Data[DataKey]);
                        NotificationMessageService.Instance().ReceivedNotification(pushType, pushChangeRequest.Data);
                        break;
                    default:
                        throw new IllegalStateException($"unknown PushType");
                }
            }
            catch (Exception e)
            {
                Log.Error(PackageName, e.Message);
            }
        }

        private void ProcessNewMessage(Conversation conversation)
        {
            using (var h = new Handler(Looper.MainLooper))
            {
                h.Post(() =>
                {
                    try
                    {
                        var formattedPhoneNumber = _contactNameProvider.GetFormattedPhoneNumber(conversation.ToPhone.PhoneNumber);
                        var contactNameOrPhone = _contactNameProvider.GetName(conversation.ToPhone.PhoneNumber);
                        ShowConversationMessagePush(
                            conversation.Id,
                            formattedPhoneNumber,
                            contactNameOrPhone,
                            conversation.Messages?.FirstOr(null)?.Text ?? "new message"
                        );
                    }
                    catch (Exception e)
                    {
                        Log.Error(PackageName, e.Message);
                    }
                });
            }
        }

        private void ShowConversationMessagePush(long conversationId, string phoneNumber, string title, string body)
        {
            var manager = NotificationManagerCompat.From(this);
            var channelId = GetString(Resource.String.DefaultNotificationChannel);

            PendingIntent pLaunchIntent;

            if (App.GetApplication(this).ApplicationHelper.ActionsHelper.IsLoggedIn)
            {
                if (App.GetApplication(this).IsAppInForeground)
                {
                    var launchIntent =
                        ChatActivity.OpenChat(this, conversationId, phoneNumber);
                    pLaunchIntent = PendingIntent.GetActivity(this, 0, launchIntent, PendingIntentFlags.UpdateCurrent);
                }
                else
                {
                    pLaunchIntent = TaskStackBuilder.Create(this)
                        .AddParentStack(Class.FromType(typeof(ContentActivity)))
                        .AddNextIntent(new Intent(this, typeof(ContentActivity)))
                        .AddNextIntent(
                            ChatActivity.OpenChat(this, conversationId, phoneNumber)
                        ).GetPendingIntent(0, (int) PendingIntentFlags.UpdateCurrent);
                }
            }
            else
            {
                pLaunchIntent = PendingIntent.GetActivity(this, 0,
                    PackageManager.GetLaunchIntentForPackage(PackageName), PendingIntentFlags.UpdateCurrent);
            }


            var notification = new NotificationCompat.Builder(this, channelId)
                .SetChannelId(channelId)
                .SetSmallIcon(Resource.Drawable.ic_default_notification)
                .SetColor(ContextCompat.GetColor(this, Resource.Color.colorActivatedControls))
                .SetAutoCancel(true)
                .SetSound(Settings.System.DefaultNotificationUri)
                .SetContentIntent(pLaunchIntent)
                .SetContentTitle(title)
                .SetContentText(body);


            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
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

            manager.Notify(Convert.ToInt32(conversationId), notification.Build());
        }
    }
}