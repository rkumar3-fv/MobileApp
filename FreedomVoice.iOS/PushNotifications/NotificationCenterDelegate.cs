using System;
using Foundation;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.Entities.Enums;
using FreedomVoice.Entities.Request;
using FreedomVoice.Entities.Response;
using FreedomVoice.iOS.Core;
using FreedomVoice.iOS.Core.Utilities.Extensions;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers;
using FreedomVoice.iOS.ViewControllers.Texts;
using UIKit;
using UserNotifications;

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
			_contactNameProvider.RequestContacts();
		}
		
		public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
		{
			_logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessStatusChangedPushNotification), "WillPresentNotification");
			completionHandler?.Invoke(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound);
		}

		public void DidReceiveSilentRemoteNotification(NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{
			_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveSilentRemoteNotification), $"userInfo: {userInfo}");

			var pushNotificationData = PushResponseExtension.CreateFrom(userInfo);
			
			if (pushNotificationData?.Data == null)
			{
				_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveSilentRemoteNotification), "Conversation is missing.");
				completionHandler?.Invoke(UIBackgroundFetchResult.Failed);
				return;
			}
			
			switch (pushNotificationData.PushType)
			{
				case PushType.NewMessage:
					_messagesService.ReceivedNotification(NotificationMessageService.NotificationType.Incoming, pushNotificationData.Data);
					completionHandler?.Invoke(UIBackgroundFetchResult.NewData);
					break;

				case PushType.StatusChanged:
					_messagesService.ReceivedNotification(NotificationMessageService.NotificationType.Update, pushNotificationData.Data);
					completionHandler?.Invoke(UIBackgroundFetchResult.NewData);
					break;
				
				default:
					_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveSilentRemoteNotification), $"Push-notification type({pushNotificationData.PushType}) is not supported");
					completionHandler?.Invoke(UIBackgroundFetchResult.Failed);
					break;
			}
		}

		public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
		{
			_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveNotificationResponse), "DidReceiveNotificationResponse");

			completionHandler?.Invoke();

			if(!UserDefault.IsAuthenticated)
				return;

			pushNotificationData = PushResponseExtension.CreateFrom(response.Notification.Request.Content.UserInfo);

			if (pushNotificationData == null)
			{
				_logger.Debug(nameof(NotificationCenterDelegate), nameof(DidReceiveNotificationResponse), "Push-notification response is null");
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

		private void ProcessStatusChangedPushNotification()
		{
			if (pushNotificationData.Data == null)
			{
				_logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessStatusChangedPushNotification), "Conversation is missing.");
				return;
			}
			
			_messagesService.ReceivedNotification(NotificationMessageService.NotificationType.Update, pushNotificationData.Data);
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
			
			if (string.IsNullOrWhiteSpace(pushNotificationData?.Data?.CollocutorPhone?.PhoneNumber))
			{
				_logger.Debug(nameof(NotificationCenterDelegate), nameof(ProcessNewMessagePushNotification), "CollocutorPhone is missing.");
				return;
			}
			
			if (string.IsNullOrWhiteSpace(pushNotificationData?.Data?.CurrentPhone?.PhoneNumber))
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
			if (pushNotificationData == null)
			{
				_appNavigator.MainTabBarControllerChanged -= MainTabBarControllerChanged;
				_appNavigator.CurrentControllerChanged -= CurrentControllerChanged;
				return;
			}
			
			if (_appNavigator.MainTabBarController == null || _appNavigator.CurrentController == null) 
				return;
			
			_appNavigator.MainTabBarControllerChanged -= MainTabBarControllerChanged;
			_appNavigator.CurrentControllerChanged -= CurrentControllerChanged;
			
			var phoneHolder = _contactNameProvider.GetNameOrNull(_contactNameProvider.GetClearPhoneNumber(pushNotificationData.Data.CollocutorPhone.PhoneNumber));

			var controller = new ConversationViewController();
			controller.ConversationId = pushNotificationData.Data.Id;
			controller.CurrentPhone = new PresentationNumber(pushNotificationData.Data.CurrentPhone.PhoneNumber);
			controller.Title = phoneHolder ?? pushNotificationData.Data.CollocutorPhone.PhoneNumber;
			_appNavigator.CurrentController.NavigationController?.PushViewController(controller, true);
			pushNotificationData = null;
		}
	}
}