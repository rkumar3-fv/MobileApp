using System;
using System.Threading.Tasks;
using Foundation;
using UserNotifications;

namespace FreedomVoice.iOS.PushNotifications
{
	/// <summary>
	/// Enum Describes possible errors during registration.
	/// </summary>
	enum PushRegistrationErrors
	{
		/// <summary>
		/// Registration is not completed or access is not denied
		/// </summary>
		accessNotGranted,
		
		/// <summary>
		/// Registration completed with a error
		/// </summary>
		registrationFailed
	}
	
	interface IPushNotificationsService
	{
		/// <summary>
		/// Method to start push notifications configuration
		/// </summary>
		/// <param name="options">Constants for requesting authorization to interact with the user.</param>
		/// <param name="registrationCompletionBlock">The method which will be invoked after registration</param>
		void RegisterForPushNotifications(UNAuthorizationOptions options, Action<PushRegistrationErrors?> registrationCompletionBlock);
		
		/// <summary>
		/// Method to start push notifications configuration
		/// </summary>
		/// <remarks>This method use the following options:: UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge </remarks>
		/// <param name="registrationCompletionBlock">The method which will be invoked after registration</param>
		void RegisterForPushNotifications(Action<PushRegistrationErrors?> registrationCompletionBlock);
		
		/// <summary>
		/// Will return current App notification settings: Granted, Not Allowed etc.
		/// </summary>
		/// <param name="completion"></param>
		void GetNotificationSettings(Action<UNNotificationSettings> completion);
		
		/// <summary>
		/// Method saves push-token and invokes registrationCompletionBlock
		/// </summary>
		/// <remarks>Must be called from AppDelegate, don't call this manually</remarks>
		/// <param name="deviceToken"></param>
		void DidRegisterForRemoteNotifications(NSData deviceToken);
		
		/// <summary>
		/// Method invokes registrationCompletionBlock with error
		/// </summary>
		/// <remarks>Must be called from AppDelegate, don't call this manually</remarks>
		/// <param name="deviceToken"></param>
		void DidFailToRegisterForRemoteNotifications(NSError error);

		/// <summary>
		/// Method registers push-token on the server.
		/// </summary>
		/// <returns></returns>
		Task RegisterPushNotificationToken();
		
		/// <summary>
		/// Method unregisters push-token on the server.
		/// </summary>
		/// <returns></returns>
		Task UnregisterPushNotificationToken();
	}
}