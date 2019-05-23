using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Utils;
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
using UIKit;
using UserNotifications;
using static FreedomVoice.iOS.Core.Utilities.Helpers.Contacts;

namespace FreedomVoice.iOS.PushNotifications
{
	class NotificationCenterDelegate: UNUserNotificationCenterDelegate
	{
		private readonly IAppNavigator _appNavigator = ServiceContainer.Resolve<IAppNavigator>();
		private readonly IContactNameProvider _contactNameProvider = ServiceContainer.Resolve<IContactNameProvider>();
		private readonly ILogger _logger = ServiceContainer.Resolve<ILogger>();
		private readonly NotificationMessageService _messagesService = NotificationMessageService.Instance();
		private PushResponse<Conversation> pushNotificationData;

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
					ProcessNewMessagePushNotification();
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
			var systemNumber = NormalizePhoneNumber(UserDefault.AccountPhoneNumber);

			var service = ServiceContainer.Resolve<IPresentationNumbersService>();
			var requestResult = await service.ExecuteRequest(systemNumber, false);
			var accountNumbers = requestResult as PresentationNumbersResponse;
			return accountNumbers.PresentationNumbers.Any(x => NormalizePhoneNumber(x.PhoneNumber) == systemNumber);
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
		
		private void ProcessNewMessagePushNotification()
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
				ShowConversationController();
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

		private void CurrentControllerChanged(BaseViewController obj)
		{
			ShowConversationController();
		}

		private void MainTabBarControllerChanged(MainTabBarController obj)
		{
			ShowConversationController();
		}

		private void ShowConversationController()
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

			var phoneHolder = _contactNameProvider.GetNameOrNull(_contactNameProvider.GetClearPhoneNumber(pushNotificationData.Data.ToPhone.PhoneNumber));

			var controller = new ConversationViewController();
			controller.ConversationId = pushNotificationData.Data.Id;
			controller.CurrentPhone = new PresentationNumber(pushNotificationData.TextMessageReceivedToNumber());
			controller.Title = phoneHolder ?? pushNotificationData.TextMessageReceivedFromNumber();
			_appNavigator.CurrentController.NavigationController?.PushViewController(controller, true);
			pushNotificationData = null;
		}
	}
}