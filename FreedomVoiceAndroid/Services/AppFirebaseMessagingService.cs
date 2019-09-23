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
using com.FreedomVoice.MobileApp.Android.Receivers;
using com.FreedomVoice.MobileApp.Android.Utils;
using Firebase.Messaging;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.Entities.Enums;
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
        private const string BadgeKey = "badge";

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
                        var newMessageRequest =
                            JsonConvert.DeserializeObject<PushResponse<Conversation>>(message.Data[DataKey]);
                        var badgeCount =
                            JsonConvert.DeserializeObject<int>(message.Data[BadgeKey]);
                        ProcessNewMessage(newMessageRequest.Data, badgeCount);
                        NotificationMessageService.Instance().ReceivedNotification(pushType, newMessageRequest.Data);
                        break;
                    case PushType.StatusChanged:
                        var pushChangeRequest =
                            JsonConvert.DeserializeObject<PushResponse<Conversation>>(message.Data[DataKey]);
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

        private void ProcessNewMessage(Conversation conversation, int badgeCount)
        {
            var myPresentationPhone = conversation?.Messages?.FirstOrDefault()?.To?.PhoneNumber;
            myPresentationPhone = _contactNameProvider.GetFormattedPhoneNumber(myPresentationPhone);

            var conversationToPhoneNumber =
                _contactNameProvider.GetFormattedPhoneNumber(conversation.ToPhone.PhoneNumber);

            new Handler(Looper.MainLooper).Post(() =>
            {
                try
                {
                    var contactNameOrPhone = _contactNameProvider.GetName(conversation.ToPhone.PhoneNumber);
                    ShowConversationMessagePush(
                        conversation.Id,
                        conversationToPhoneNumber,
                        myPresentationPhone,
                        contactNameOrPhone,
                        conversation.Messages?.FirstOr(null)?.Text ?? "new message",
                        badgeCount
                    );
                }
                catch (Exception e)
                {
                    Log.Error(PackageName, e.Message);
                }
            });
        }

        private void ShowConversationMessagePush(long conversationId, string fromPhone, string myPhone, string title, string body, int badgeCount)
        {
            var manager = NotificationManagerCompat.From(this);

            var intent = new Intent(this, typeof(NotificationBroadcastReceiver));
            var openChatIntent = ChatActivity.OpenChat(this, conversationId, fromPhone, myPhone);
            intent.PutExtra(BaseActivity.NavigationRedirectActivityName, Class.FromType(typeof(ChatActivity)).Name);
            intent.PutExtra(BaseActivity.NavigatePayloadBundle, openChatIntent.Extras);

            var pLaunchIntent = PendingIntent.GetBroadcast(this, 0, intent, PendingIntentFlags.UpdateCurrent);

            var notification = NotificationUtils.GetDefaultBuilder(this)
                .SetSmallIcon(Resource.Drawable.ic_default_notification)
                .SetColor(ContextCompat.GetColor(this, Resource.Color.colorActivatedControls))
                .SetAutoCancel(true)
                .SetSound(Settings.System.DefaultNotificationUri)
                .SetContentIntent(pLaunchIntent)
                .SetContentTitle(title)
                .SetNumber(badgeCount)
                .SetSubText(myPhone)
                .SetContentText(body);
            manager.Notify(Convert.ToInt32(conversationId), notification.Build());
        }
    }
}