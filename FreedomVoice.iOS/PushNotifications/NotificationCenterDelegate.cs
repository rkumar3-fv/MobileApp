using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.Entities.Enums;
using FreedomVoice.Entities.PushContract;
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
using ContactsHelper = FreedomVoice.iOS.Core.Utilities.Helpers.Contacts;

namespace FreedomVoice.iOS.PushNotifications
{
	class NotificationCenterDelegate: UNUserNotificationCenterDelegate, IPKPushRegistryDelegate
	{
		private readonly IAppNavigator _appNavigator = ServiceContainer.Resolve<IAppNavigator>();
		private readonly IContactNameProvider _contactNameProvider = ServiceContainer.Resolve<IContactNameProvider>();
		private readonly ILogger _logger = ServiceContainer.Resolve<ILogger>();
		private readonly NotificationMessageService _messagesService = NotificationMessageService.Instance();
		private PushResponse<Conversation> pushNotificationData;
		
		private readonly NSString ApsKey = new NSString("aps");
		private readonly NSString ContentAvailableKey = new NSString("content-available");
		private readonly NSString AlertKey = new NSString("alert");
		private readonly NSString TitleKey = new NSString("title");
		private readonly NSString BodyKey = new NSString("body");
		
		public event Action<NSData> DidUpdatePushToken;

		public NotificationCenterDelegate()
		{
			UserDefault.IsAuthenticatedChanged += IsAuthenticatedChanged;
		}

		public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
		{
			_logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessStatusChangedPushNotification), "WillPresentNotification");
			completionHandler?.Invoke(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound);
			DidReceiveSilentRemoteNotification(notification.Request.Content.UserInfo, null);
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

			completionHandler?.Invoke();

			if (!UserDefault.IsAuthenticated)
			{
				completionHandler?.Invoke();
				_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveNotificationResponse), "User is not authorized");
				return;
			}

			pushNotificationData = PushResponseExtension.CreateFromFromJson(response.Notification.Request.Content.UserInfo);

			if (pushNotificationData == null)
			{
				_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveNotificationResponse), "Push-notification response is null");
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
		}

		private async Task<bool> CheckCurrentNumber()
		{
			var systemNumber = ContactsHelper.NormalizePhoneNumber(UserDefault.AccountPhoneNumber);

			var service = ServiceContainer.Resolve<IPresentationNumbersService>();
			var requestResult = await service.ExecuteRequest(systemNumber, false);
			var accountNumbers = requestResult as PresentationNumbersResponse;
			return accountNumbers.PresentationNumbers.Any(x => ContactsHelper.NormalizePhoneNumber(x.PhoneNumber) == systemNumber);
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
				await ContactsHelper.GetContactsListAsync();
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
 			_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), $"DidUpdatePushCredentials: {payload} {type}");

			if (!payload.DictionaryPayload.ContainsKey(new NSString(ApsKey)))
			{
				_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), "APS key is missing");
				return;
			}
			
			var apsValue = payload.DictionaryPayload[ApsKey] as NSDictionary;
			
			if (apsValue.ContainsKey(new NSString(ContentAvailableKey)) && apsValue[ContentAvailableKey] is NSNumber contentAvailable && contentAvailable.Int32Value == 1)
			{
				DidReceiveSilentRemoteNotification(payload.DictionaryPayload, null);
				return;
			}

			if (!apsValue.ContainsKey(new NSString(AlertKey)))
			{
				_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), "Alert key is missing");
				return;
			}

			string titleValue = null;
			string bodyValue = null;

			switch (apsValue[AlertKey])
			{
				case NSDictionary alertValue when !alertValue.ContainsKey(new NSString(TitleKey)) || !alertValue.ContainsKey(new NSString(BodyKey)):
					_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), $"APS doesn't have title and body");
					return;
				
				case NSDictionary alertValue:
					titleValue = alertValue[TitleKey] as NSString;
					bodyValue = alertValue[BodyKey] as NSString;
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
				ShowPushNotificationsNow(titleValue, bodyValue, payload.DictionaryPayload);
				return;
			}

			var fromPhone = pushResponseData.TextMessageReceivedFromNumber();
			var phoneHolder = _contactNameProvider.GetNameOrNull(ContactsHelper.NormalizePhoneNumber(fromPhone));

			if (string.IsNullOrWhiteSpace(phoneHolder))
			{
				_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), $"Can't parse 'From phone number'.");
				ShowPushNotificationsNow(titleValue, bodyValue, payload.DictionaryPayload);
				return;
			}

			_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidUpdatePushCredentials), $"'From' phone number has been found: {phoneHolder}");
			ShowPushNotificationsNow(phoneHolder, bodyValue, payload.DictionaryPayload);
		}

		private void ShowPushNotificationsNow(string title, string body, NSDictionary userInfo)
		{
			_logger.Debug(nameof(NotificationCenterDelegate), nameof(ShowPushNotificationsNow), $"Show alerts as title: {title}, body: {body}");

			var notificationContent = new UNMutableNotificationContent();
            if (title != null) notificationContent.Title = title;
            if (body != null) notificationContent.Body = body;
            if (userInfo != null) notificationContent.UserInfo = userInfo;
            notificationContent.Sound = UNNotificationSound.Default;

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
			var localNotificationRequest = UNNotificationRequest.FromIdentifier(new NSUuid().AsString(), notificationContent, trigger);
			
			UNUserNotificationCenter.Current.AddNotificationRequest(localNotificationRequest, error =>
			{
				_logger.Debug(nameof(NotificationCenterDelegate), nameof(ShowPushNotificationsNow), $"Push has been showed. Error: {error}");
			});
		}
		
		#endregion

	}
}