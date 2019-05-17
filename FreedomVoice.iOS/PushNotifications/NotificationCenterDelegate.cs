using System;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.PushNotifications.PushModel;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers;
using FreedomVoice.iOS.ViewControllers.Texts;
using Microsoft.Extensions.Options;
using UIKit;
using UserNotifications;

namespace FreedomVoice.iOS.PushNotifications
{
	class NotificationCenterDelegate: UNUserNotificationCenterDelegate
	{
		private readonly IAppNavigator _appNavigator = ServiceContainer.Resolve<IAppNavigator>();
		private readonly IContactNameProvider _contactNameProvider = ServiceContainer.Resolve<IContactNameProvider>();
		private PushNotification pushNotificationData;

		public NotificationCenterDelegate()
		{
			UserDefault.IsAuthenticatedChanged += IsAuthenticatedChanged;
			_contactNameProvider.RequestContacts();
		}
		
		public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
		{
			Console.WriteLine($"[{GetType()}] DidReceiveNotificationResponse");
			completionHandler?.Invoke();

			if(!UserDefault.IsAuthenticated)
				return;
			
			pushNotificationData = new PushNotification(response.Notification.Request.Content);
			if (
				!(pushNotificationData.data?.message?.conversationId).HasValue ||
				string.IsNullOrWhiteSpace(pushNotificationData.data?.message?.fromPhoneNumber) ||
				string.IsNullOrWhiteSpace(pushNotificationData.data?.message?.toPhoneNumber))
			{
				Console.WriteLine($"[{GetType()}] ConversationId is missing.");
				return;
			}

			if (_appNavigator.MainTabBarController != null && _appNavigator.CurrentController != null)
			{
				Console.WriteLine($"[{GetType()}] ShowConversationController");
				ShowConversationController();
			}
			else
			{
				Console.WriteLine($"[{GetType()}] MainTabBarController is not prepared yet. Waiting MainTabBarControllerChanged");
				_appNavigator.MainTabBarControllerChanged += MainTabBarControllerChanged;
				_appNavigator.CurrentControllerChanged += CurrentControllerChanged;
			}
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
			
			var phoneHolder = _contactNameProvider.GetNameOrNull(_contactNameProvider.GetClearPhoneNumber(pushNotificationData.data?.message?.fromPhoneNumber));

			var controller = new ConversationViewController();
			controller.ConversationId = pushNotificationData.data.message.conversationId;
			controller.CurrentPhone = new PresentationNumber(pushNotificationData.data.message.toPhoneNumber);
			controller.Title = phoneHolder ?? pushNotificationData.data.message.fromPhoneNumber;
			_appNavigator.CurrentController.NavigationController?.PushViewController(controller, true);
			pushNotificationData = null;
		}

		public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
		{
			Console.WriteLine($"[{GetType()}] WillPresentNotification");
			completionHandler?.Invoke(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound);
		}
	}
}