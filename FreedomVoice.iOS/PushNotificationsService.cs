using System;
using CoreFoundation;
using Foundation;
using UIKit;
using UserNotifications;

namespace FreedomVoice.iOS
{
	enum PushRegistrationErrors
	{
		accessNotGranted,
		registrationFailed
	}

	interface IPushNotificationsService
	{
		void RegisterForPushNotifications(UNAuthorizationOptions options, Action<PushRegistrationErrors?> registrationCompletionBlock);
		void GetNotificationSettings(Action<UNNotificationSettings> completion);
		void DidRegisterForRemoteNotifications(NSData deviceToken);
		void DidFailToRegisterForRemoteNotifications(NSError error);
	}
	
	class PushNotificationsService: IPushNotificationsService
	{
		private UNUserNotificationCenter notificationCenter = UNUserNotificationCenter.Current;
		private Action<PushRegistrationErrors?> registrationCompletionBlock;
		
		///Will hold device token in Data representations
		private NSData tokenRawData;

		///Method to start push notifications configuration
		public void RegisterForPushNotifications(UNAuthorizationOptions options, Action<PushRegistrationErrors?> registrationCompletionBlock)
		{
			notificationCenter.RequestAuthorization(options, (granted, error) =>
			{
				if (!granted)
				{
					registrationCompletionBlock?.Invoke(PushRegistrationErrors.accessNotGranted);
					return;
				}

				notificationCenter.GetNotificationSettings((settings) =>
				{
					if (settings.AuthorizationStatus != UNAuthorizationStatus.Authorized)
					{
						registrationCompletionBlock?.Invoke(PushRegistrationErrors.accessNotGranted);
						return;
					}

					this.registrationCompletionBlock = registrationCompletionBlock;

					DispatchQueue.MainQueue.DispatchAsync(() =>
					{
						UIApplication.SharedApplication.RegisterForRemoteNotifications();
					});
				});
			});
		}
		
		///will return current App notification settings: Granted, Not Allowed etc.
		public void GetNotificationSettings(Action<UNNotificationSettings> completion)
		{
			notificationCenter.GetNotificationSettings(settings => { completion?.Invoke(settings); });
		}

		///Called from AppDelegate, don't call this manually
		public void DidRegisterForRemoteNotifications(NSData deviceToken)
		{
			var token = deviceToken.Description.Trim('<').Trim('>').Replace(" ", "");
			Console.WriteLine($"DidRegisterForRemoteNotifications: {token}");
			tokenRawData = deviceToken;
			registrationCompletionBlock?.Invoke(null);
		}

		///Called from AppDelegate, don't call this manually
		public void DidFailToRegisterForRemoteNotifications(NSError error)
		{
			registrationCompletionBlock?.Invoke(PushRegistrationErrors.registrationFailed);
		}
	}

	class NotificationCenterDelegate: UNUserNotificationCenterDelegate
	{
		public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
		{
			completionHandler?.Invoke();
		}

		public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
		{
			completionHandler?.Invoke(UNNotificationPresentationOptions.Alert);
		}
	}

}