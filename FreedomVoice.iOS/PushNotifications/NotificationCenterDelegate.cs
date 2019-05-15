using System;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.PushNotifications.PushModel;
using FreedomVoice.iOS.ViewControllers;
using FreedomVoice.iOS.ViewControllers.Texts;
using Microsoft.Extensions.Options;
using UIKit;
using UserNotifications;

namespace FreedomVoice.iOS.PushNotifications
{
	class NotificationCenterDelegate: UNUserNotificationCenterDelegate
	{
		public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
		{
			Console.WriteLine($"[{GetType()}] DidReceiveNotificationResponse");
			completionHandler?.Invoke();

			var pushNotificationData = new PushNotification(response.Notification.Request.Content);
			if (
				!(pushNotificationData.data?.message?.conversationId).HasValue ||
				string.IsNullOrWhiteSpace(pushNotificationData.data?.message?.fromPhoneNumber) ||
				string.IsNullOrWhiteSpace(pushNotificationData.data?.message?.toPhoneNumber))
			{
				Console.WriteLine($"[{GetType()}] ConversationId is missing.");
				return;
			}

			Console.WriteLine($"[{GetType()}] Try to show conversation page");
			var controller = new ConversationViewController();
			controller.ConversationId = pushNotificationData.data.message.conversationId;
			controller.CurrentPhone = new PresentationNumber(pushNotificationData.data.message.toPhoneNumber);
			controller.Title = pushNotificationData.data?.message?.fromPhoneNumber;
			
			ServiceContainer.Resolve<IAppNavigator>().CurrentController.NavigationController.PushViewController(controller, true);
		}

		public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
		{
			Console.WriteLine($"[{GetType()}] WillPresentNotification");
			completionHandler?.Invoke(UNNotificationPresentationOptions.Alert);
		}
	}

	public interface IAppNavigator
	{
		BaseViewController CurrentController { get; }
		void Configure(AppDelegate app);

		void UpdateCurrentController(BaseViewController currentController);

	}
	
	public class AppNavigator: IAppNavigator
	{
		public BaseViewController CurrentController { get; private set; }

		public AppNavigator()
		{
		}

		public void Configure(AppDelegate app)
		{
			app.Window = new UIWindow(UIScreen.MainScreen.Bounds);

			app.Window.BackgroundColor = UIColor.White;
			app.Window.MakeKeyAndVisible();
		}

		public void UpdateCurrentController(BaseViewController currentController)
		{
			CurrentController = currentController;
		}
	}
}