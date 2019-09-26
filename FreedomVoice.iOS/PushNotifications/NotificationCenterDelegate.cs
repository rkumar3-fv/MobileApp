using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.Entities.Enums;
using FreedomVoice.Entities.Response;
using FreedomVoice.iOS.Core;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers;
using FreedomVoice.iOS.ViewControllers.Texts;
using PushKit;
using UIKit;
using UserNotifications;

namespace FreedomVoice.iOS.PushNotifications
{
    class NotificationCenterDelegate : UNUserNotificationCenterDelegate, IPKPushRegistryDelegate
    {
        private readonly IAppNavigator _appNavigator = ServiceContainer.Resolve<IAppNavigator>();
        private readonly IContactNameProvider _contactNameProvider = ServiceContainer.Resolve<IContactNameProvider>();
        private readonly ILogger _logger = ServiceContainer.Resolve<ILogger>();
        private readonly IPhoneFormatter _phoneFormatter = ServiceContainer.Resolve<IPhoneFormatter>();
        private readonly NotificationMessageService _messagesService = NotificationMessageService.Instance();
        private PushResponse<Conversation> pushNotificationData;

        private readonly NSString ApsKey = new NSString("aps");
        private readonly NSString ContentAvailableKey = new NSString("content-available");
        private readonly NSString AlertKey = new NSString("alert");
        private readonly NSString TitleKey = new NSString("title");
        private readonly NSString BodyKey = new NSString("body");
        private readonly NSString SubtitleKey = new NSString("subtitle");
        private readonly NSString BadgeKey = new NSString("badge");
        private readonly IPresentationNumbersService _presentationNumbersService = ServiceContainer.Resolve<IPresentationNumbersService>();

        public event Action<NSData> DidUpdatePushToken;

        public NotificationCenterDelegate()
        {
            UserDefault.IsAuthenticatedChanged += IsAuthenticatedChanged;
        }

        public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            _logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessStatusChangedPushNotification), "WillPresentNotification");
            DidReceiveSilentRemoteNotification(notification.Request.Content.UserInfo, null);
            completionHandler?.Invoke(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound);
        }

        public async void DidReceiveSilentRemoteNotification(NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveSilentRemoteNotification), $"userInfo: {userInfo}");

            var pushNotificationData = PushResponseExtension.CreateFromFromJson(userInfo);

            if (pushNotificationData?.Data == null)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveSilentRemoteNotification), "Conversation is missing.");
                completionHandler?.Invoke(UIBackgroundFetchResult.Failed);
                return;
            }

            if (!await CheckCurrentNumber())
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveSilentRemoteNotification), "Push recipient is not current user");
                completionHandler?.Invoke(UIBackgroundFetchResult.Failed);
                return;
            }

            switch (pushNotificationData.PushType)
            {
                case PushType.NewMessage:
                case PushType.StatusChanged:
                    _messagesService.ReceivedNotification(pushNotificationData.PushType, pushNotificationData.Data);
                    completionHandler?.Invoke(UIBackgroundFetchResult.NewData);
                    break;

                default:
                    _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveSilentRemoteNotification), $"Push-notification type({pushNotificationData.PushType}) is not supported");
                    completionHandler?.Invoke(UIBackgroundFetchResult.Failed);
                    break;
            }
        }

        public override async void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveNotificationResponse), "DidReceiveNotificationResponse");

            if (!UserDefault.IsAuthenticated)
            {
                completionHandler?.Invoke();
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveNotificationResponse), "User is not authorized");
                completionHandler?.Invoke();
                return;
            }

            pushNotificationData = PushResponseExtension.CreateFromFromJson(response.Notification.Request.Content.UserInfo);

            if (pushNotificationData == null)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveNotificationResponse), "Push-notification response is null");
                completionHandler?.Invoke();
                return;
            }

            if (!await CheckCurrentNumber())
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveNotificationResponse), "Push recipient is not current user");
                completionHandler?.Invoke();
                return;
            }

            switch (pushNotificationData.PushType)
            {
                case PushType.NewMessage:
                    await ProcessNewMessagePushNotification();
                    break;

                case PushType.StatusChanged:
                    ProcessStatusChangedPushNotification();
                    break;

                default:
                    _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveNotificationResponse), $"Push-notification type({pushNotificationData.PushType}) is not supported");
                    break;
            }
            completionHandler?.Invoke();
        }

        private async Task<bool> CheckCurrentNumber()
        {
            try
            {
                if (string.IsNullOrEmpty(UserDefault.LastUsedAccount))
                    return false;

                var systemNumber = _phoneFormatter.Normalize(UserDefault.LastUsedAccount);

                if (string.IsNullOrEmpty(systemNumber))
                    return false;

                var requestResult = await _presentationNumbersService.ExecuteRequest(systemNumber, false);
                var accountNumbers = requestResult as PresentationNumbersResponse;
                return accountNumbers?.PresentationNumbers?.Any(x => _phoneFormatter.Normalize(x.PhoneNumber) == systemNumber) ?? false;
            }
            catch (Exception ex)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(CheckCurrentNumber), ex.Message);
                return false;
            }
        }

        private void ProcessStatusChangedPushNotification()
        {
            if (pushNotificationData.Data == null)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessStatusChangedPushNotification), "Conversation is missing.");
                return;
            }

            _messagesService.ReceivedNotification(pushNotificationData.PushType, pushNotificationData.Data);
            pushNotificationData = null;
            _logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessStatusChangedPushNotification), "StatusChanged notification has been processed.");
        }

        private async Task ProcessNewMessagePushNotification()
        {
            if (pushNotificationData.Data == null)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessNewMessagePushNotification), "Conversation is missing.");
                return;
            }

            if (!(pushNotificationData.Data?.Id).HasValue)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessNewMessagePushNotification), "Conversation id is missing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(pushNotificationData?.TextMessageReceivedFromNumber()))
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessNewMessagePushNotification), "CollocutorPhone is missing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(pushNotificationData?.TextMessageReceivedToNumber()))
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessNewMessagePushNotification), "CurrentPhone is missing.");
                return;
            }

            if (_appNavigator.MainTabBarController != null || _appNavigator.CurrentController != null)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessNewMessagePushNotification), "ShowConversationController");
                await ShowConversationController();
            }
            else
            {
                _appNavigator.MainTabBarControllerChanged -= MainTabBarControllerChanged;
                _appNavigator.CurrentControllerChanged -= CurrentControllerChanged;

                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessNewMessagePushNotification), "MainTabBarController is not prepared yet. Waiting MainTabBarControllerChanged");

                _appNavigator.MainTabBarControllerChanged += MainTabBarControllerChanged;
                _appNavigator.CurrentControllerChanged += CurrentControllerChanged;
            }
            _logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessNewMessagePushNotification), "NewMessage notification has been processed.");

        }

        private void IsAuthenticatedChanged()
        {
            if (UserDefault.IsAuthenticated) return;

            _appNavigator.MainTabBarControllerChanged -= MainTabBarControllerChanged;
            _appNavigator.CurrentControllerChanged -= CurrentControllerChanged;
            pushNotificationData = null;
        }

        private async void CurrentControllerChanged(BaseViewController obj)
        {
            await ShowConversationController();
        }

        private async void MainTabBarControllerChanged(MainTabBarController obj)
        {
            await ShowConversationController();
        }

        private async Task ShowConversationController()
        {
            if (pushNotificationData?.Data == null)
            {
                pushNotificationData = null;
                _appNavigator.MainTabBarControllerChanged -= MainTabBarControllerChanged;
                _appNavigator.CurrentControllerChanged -= CurrentControllerChanged;
                return;
            }

            if (_appNavigator.MainTabBarController == null || _appNavigator.CurrentController == null)
                return;

            if (_appNavigator.CurrentController is ConversationViewController)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ShowConversationController), "Conversation page have already opened)");
                pushNotificationData = null;
                _appNavigator.MainTabBarControllerChanged -= MainTabBarControllerChanged;
                _appNavigator.CurrentControllerChanged -= CurrentControllerChanged;
                return;
            }

            _appNavigator.MainTabBarControllerChanged -= MainTabBarControllerChanged;
            _appNavigator.CurrentControllerChanged -= CurrentControllerChanged;

            try
            {
                await FreedomVoice.iOS.Core.Utilities.Helpers.Contacts.GetContactsListAsync();
            }
            catch (Exception ex)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ShowConversationController), $"GetContactsListAsync failed: {ex}");
            }

            var phoneHolder = _contactNameProvider.GetNameOrNull(_contactNameProvider.GetClearPhoneNumber(pushNotificationData.Data.ToPhone.PhoneNumber));
            var controller = new ConversationViewController();
            controller.ConversationId = pushNotificationData.Data.Id;
            controller.CurrentPhone = new PresentationNumber(pushNotificationData.TextMessageReceivedToNumber());
            controller.Title = phoneHolder ?? pushNotificationData.TextMessageReceivedFromNumber();
            _appNavigator.CurrentController.NavigationController?.PushViewController(controller, true);
            pushNotificationData = null;
        }

        #region IPKPushRegistryDelegate implementation

        public void DidUpdatePushCredentials(PKPushRegistry registry, PKPushCredentials credentials, string type)
        {
            _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), $"DidUpdatePushCredentials: {credentials.Token} {type}");
            DidUpdatePushToken?.Invoke(credentials.Token);
        }

        public void DidReceiveIncomingPush(PKPushRegistry registry, PKPushPayload payload, string type)
        {
            int badgeCount = 0;
            _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), $"DidUpdatePushCredentials: {payload} {type}");

            if (!payload.DictionaryPayload.ContainsKey(new NSString(ApsKey)))
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), "APS key is missing");
                return;
            }

            if (payload.DictionaryPayload.ContainsKey(new NSString(BadgeKey)))
            {
                var badge = payload.DictionaryPayload[BadgeKey] as NSNumber;
                badgeCount = badge.Int32Value;
            }
            var apsValue = payload.DictionaryPayload[ApsKey] as NSDictionary;

            if (apsValue.ContainsKey(new NSString(ContentAvailableKey)) && apsValue[ContentAvailableKey] is NSNumber contentAvailable && contentAvailable.Int32Value == 1)
            {
                DidReceiveSilentRemoteNotification(payload.DictionaryPayload, null);
                return;
            }

            if (UIApplication.SharedApplication.ApplicationState != UIApplicationState.Active)
            {
                DidReceiveSilentRemoteNotification(payload.DictionaryPayload, null);
            }

            if (!apsValue.ContainsKey(new NSString(AlertKey)))
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), "Alert key is missing");
                return;
            }

            string titleValue = null;
            string bodyValue = null;
            string subtitleValue = null;

            switch (apsValue[AlertKey])
            {
                case NSDictionary alertValue:
                    titleValue = alertValue[TitleKey] as NSString;
                    bodyValue = alertValue[BodyKey] as NSString;
                    subtitleValue = alertValue[SubtitleKey] as NSString;

                    break;

                case NSString apsTitle:
                    titleValue = apsTitle;
                    break;

                default:
                    _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), "APS doesn't have title content");
                    return;
            }

            var pushResponseData = PushResponseExtension.CreateFromFromJson(payload.DictionaryPayload);
            if (pushResponseData == null)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), $"Can't parse Data(PushResponse).");
                ShowPushNotificationsNow(titleValue, bodyValue, subtitleValue, payload.DictionaryPayload, badgeCount);
                return;
            }

            var fromPhone = pushResponseData.TextMessageReceivedFromNumber();
            var phoneHolder = _contactNameProvider.GetNameOrNull(_phoneFormatter.Normalize(fromPhone));

            if (string.IsNullOrWhiteSpace(phoneHolder))
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), $"Can't parse 'From phone number'.");
                ShowPushNotificationsNow(titleValue, bodyValue, subtitleValue, payload.DictionaryPayload, badgeCount);
                return;
            }

            _logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), $"'From' phone number has been found: {phoneHolder}");
            ShowPushNotificationsNow(phoneHolder, bodyValue, subtitleValue, payload.DictionaryPayload, badgeCount);

        }

        private void ShowPushNotificationsNow(string title, string body, string subtitle, NSDictionary userInfo, int badgeCount)
        {
            _logger.Debug(nameof(NotificationCenterDelegate), nameof(ShowPushNotificationsNow), $"Show alerts as title: {title}, subtitle: {subtitle} body: {body}");
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = badgeCount;

            if (UIDevice.CurrentDevice.CheckSystemVersion(12, 0))
                ShowPushNotificationsNowForiOS12AndLater(title, body, subtitle, userInfo, badgeCount);
            else
                ShowPushNotificationsNowForiOS11AndLess(title, body, subtitle, userInfo, badgeCount);

        }

        private void ShowPushNotificationsNowForiOS12AndLater(string title, string body, string subtitle, NSDictionary userInfo, int badgeCount)
        {
            try
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ShowPushNotificationsNowForiOS12AndLater), $"Show alert for iOS 12 and later.");

                var notificationContent = new UNMutableNotificationContent();
                if (title != null) notificationContent.Title = title;
                if (body != null) notificationContent.Body = body;
                if (subtitle != null) notificationContent.Subtitle = subtitle;
                if (userInfo != null) notificationContent.UserInfo = userInfo;
                notificationContent.Sound = UNNotificationSound.Default;
                if(badgeCount >0) notificationContent.Badge = badgeCount;

                var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
                var localNotificationRequest = UNNotificationRequest.FromIdentifier(new NSUuid().AsString(), notificationContent, trigger);

                UNUserNotificationCenter.Current.AddNotificationRequest(localNotificationRequest, error =>
                {
                    _logger.Debug(nameof(NotificationCenterDelegate), nameof(ShowPushNotificationsNow), $"Push has been showed. Error: {error}");
                });
            }
            catch (Exception ex)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ShowPushNotificationsNowForiOS12AndLater), $"Cannot show alert. Error: {ex.Message}");
            }
        }

        private void ShowPushNotificationsNowForiOS11AndLess(string title, string body, string subtitle, NSDictionary userInfo, int badgeCount)
        {
            _logger.Debug(nameof(NotificationCenterDelegate), nameof(ShowPushNotificationsNowForiOS11AndLess), $"Show alert for iOS 11 and less.");

            try
            {
                UILocalNotification notification = new UILocalNotification();
                notification.TimeZone = NSTimeZone.SystemTimeZone;
                notification.FireDate = NSDate.Now.AddSeconds(1);

                if (title != null) notification.AlertTitle = title;
                if (userInfo != null) notification.UserInfo = userInfo;
                if (badgeCount > 0) notification.ApplicationIconBadgeNumber = badgeCount;
                notification.AlertBody = $"{(string.IsNullOrWhiteSpace(subtitle) ? "" : subtitle + "\n")}{body ?? ""}";

                notification.SoundName = UILocalNotification.DefaultSoundName;
                UIApplication.SharedApplication.ScheduleLocalNotification(notification);
            }
            catch (Exception ex)
            {
                _logger.Debug(nameof(NotificationCenterDelegate), nameof(ShowPushNotificationsNowForiOS11AndLess), $"Cannot show alert. Error: {ex.Message}");
            }
        }

        #endregion

    }
}
